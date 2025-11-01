#!/usr/bin/env python3
"""Scan ONI mods for event usage anti-patterns."""
from __future__ import annotations

import json
import os
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, Iterable, List, Tuple

REPO_ROOT = Path(__file__).resolve().parents[1]
SKIP_DIRS = {"bin", "obj", ".git"}
MOD_CONFIG_FILES = ("mod.yaml", "mod_info.json")

PATTERNS: Dict[str, re.Pattern[str]] = {
    "A": re.compile(r"\bSubscribe\s*\([^)]*GameHashes\."),
    "B": re.compile(r"\bUnsubscribe\s*\([^)]*GameHashes\."),
    "C": re.compile(r"\bTrigger\s*\(\s*\(int\)\s*GameHashes\."),
    "D": re.compile(r"\([^\)]+\)\s*data\b"),
    "E": re.compile(r"=>|\bdelegate\b"),
}


class ModParseError(RuntimeError):
    """Raised when mod metadata cannot be parsed."""


@dataclass
class Finding:
    category: str
    line: int
    text: str


@dataclass
class FileFindings:
    path: str
    findings: List[Finding] = field(default_factory=list)


@dataclass
class ModReport:
    id: str
    title: str
    path: str
    files: List[FileFindings] = field(default_factory=list)
    counts: Dict[str, int] = field(default_factory=dict)

    @property
    def files_touched(self) -> int:
        return len(self.files)


def discover_mods(root: Path) -> List[Tuple[Path, Dict[str, str]]]:
    mods: Dict[Path, Dict[str, str]] = {}
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
        current = Path(dirpath)
        for config_name in MOD_CONFIG_FILES:
            if config_name in filenames:
                if current not in mods:
                    metadata = parse_mod_metadata(current, config_name)
                    mods[current] = metadata
                break
    return sorted((path, data) for path, data in mods.items())


def parse_mod_metadata(mod_root: Path, config_name: str) -> Dict[str, str]:
    config_path = mod_root / config_name
    try:
        if config_name.endswith(".json"):
            data = json.loads(config_path.read_text(encoding="utf-8"))
            mod_id = str(data.get("staticID") or data.get("id") or "").strip()
            title = str(data.get("title") or "").strip()
        else:
            mod_id, title = parse_simple_yaml(config_path)
    except json.JSONDecodeError as exc:
        raise ModParseError(f"Failed to parse JSON in {config_path}: {exc}") from exc
    except OSError as exc:
        raise ModParseError(f"Unable to read {config_path}: {exc}") from exc

    if not mod_id:
        mod_id = mod_root.name
    if not title:
        title = mod_root.name
    return {"id": mod_id, "title": title, "config": str(config_path.relative_to(REPO_ROOT))}


def parse_simple_yaml(path: Path) -> Tuple[str, str]:
    mod_id = ""
    title = ""
    try:
        lines = path.read_text(encoding="utf-8").splitlines()
    except OSError as exc:
        raise ModParseError(f"Unable to read {path}: {exc}") from exc
    key_pattern = re.compile(r"^(?P<key>[A-Za-z0-9_]+)\s*:\s*(?P<value>.+?)\s*$")
    for raw_line in lines:
        line = raw_line.strip()
        if not line or line.startswith("#"):
            continue
        match = key_pattern.match(line)
        if not match:
            continue
        key = match.group("key")
        value = match.group("value").split("#", 1)[0].strip()
        if len(value) >= 2 and ((value.startswith("'") and value.endswith("'")) or (value.startswith('"') and value.endswith('"'))):
            value = value[1:-1]
        if key in {"staticID", "id"} and not mod_id:
            mod_id = value
        elif key == "title" and not title:
            title = value
    return mod_id, title


def scan_mod(mod_root: Path, metadata: Dict[str, str]) -> ModReport:
    files: List[FileFindings] = []
    counts = {key: 0 for key in PATTERNS}
    for cs_path in sorted(mod_root.rglob("*.cs")):
        if any(part in SKIP_DIRS for part in cs_path.parts):
            continue
        try:
            content = cs_path.read_text(encoding="utf-8", errors="ignore").splitlines()
        except OSError:
            continue
        file_findings: List[Finding] = []
        for idx, line in enumerate(content, start=1):
            stripped = line.strip()
            if not stripped:
                continue
            for category, pattern in PATTERNS.items():
                if pattern.search(line):
                    file_findings.append(Finding(category, idx, stripped))
                    counts[category] += 1
        if file_findings:
            files.append(FileFindings(
                path=str(cs_path.relative_to(REPO_ROOT)),
                findings=file_findings,
            ))
    return ModReport(
        id=metadata["id"],
        title=metadata["title"],
        path=str(mod_root.relative_to(REPO_ROOT)),
        files=files,
        counts=counts,
    )


def build_report(mod_reports: Iterable[ModReport]) -> Dict[str, object]:
    totals = {key: 0 for key in PATTERNS}
    totals["mods"] = 0
    totals["files_touched"] = 0
    mods_payload = []
    for report in mod_reports:
        totals["mods"] += 1
        totals["files_touched"] += report.files_touched
        for key in PATTERNS:
            totals[key] += report.counts.get(key, 0)
        mods_payload.append({
            "id": report.id,
            "title": report.title,
            "path": report.path,
            "counts": report.counts,
            "files": [
                {
                    "path": file.path,
                    "findings": [
                        {"category": finding.category, "line": finding.line, "text": finding.text}
                        for finding in file.findings
                    ],
                }
                for file in report.files
            ],
        })
    return {"mods": mods_payload, "totals": totals}


def print_summary(mod_reports: Iterable[ModReport]) -> None:
    headers = ["mod_id", "A", "B", "C", "D", "E", "files_touched"]
    rows = []
    for report in mod_reports:
        row = [
            report.id,
            str(report.counts.get("A", 0)),
            str(report.counts.get("B", 0)),
            str(report.counts.get("C", 0)),
            str(report.counts.get("D", 0)),
            str(report.counts.get("E", 0)),
            str(report.files_touched),
        ]
        rows.append(row)
    widths = [len(h) for h in headers]
    for row in rows:
        for idx, cell in enumerate(row):
            widths[idx] = max(widths[idx], len(cell))
    header_line = " | ".join(h.ljust(widths[idx]) for idx, h in enumerate(headers))
    separator = "-+-".join("-" * widths[idx] for idx in range(len(headers)))
    print(header_line)
    print(separator)
    for row in rows:
        print(" | ".join(cell.ljust(widths[idx]) for idx, cell in enumerate(row)))


def write_findings(report: Dict[str, object]) -> None:
    output_path = REPO_ROOT / "findings.json"
    with output_path.open("w", encoding="utf-8") as fh:
        json.dump(report, fh, indent=2, ensure_ascii=False)
        fh.write("\n")


def main() -> int:
    mods = discover_mods(REPO_ROOT)
    mod_reports = [scan_mod(path, metadata) for path, metadata in mods]
    report = build_report(mod_reports)
    write_findings(report)
    print_summary(mod_reports)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ModParseError as exc:
        print(str(exc), file=sys.stderr)
        raise SystemExit(1)
