#!/usr/bin/env python3
"""Scan the repository for reflection hotspots and rebuild ``ToDo.md``."""
from __future__ import annotations

import argparse
import re
from dataclasses import dataclass
from collections.abc import Iterable
from itertools import groupby
from operator import attrgetter
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
TODO_PATH = REPO_ROOT / "ToDo.md"
DEFAULT_PROJECTS = (
    "Sgt_Imalas-Oni-Mods",
    "ONI_Mods_byPether",
    "Oni_mods_by_Identifier",
    "src",
)

HOT_TOKENS = (
    "Update",
    "SimUpdate",
    "LateUpdate",
    "FastUpdate",
    "Refresh",
    "OnSpawn",
    "OnPrefabInit",
    "Prefix",
    "Postfix",
    "Transpiler",
)

REFLECTION_PATTERN = re.compile(
    r"MethodInfo\.Invoke|"
    r"PropertyInfo\.(?:GetValue|SetValue)|"
    r"FieldInfo\.(?:GetValue|SetValue)|"
    r"Get(?:Property|Field|Method)\(",
    flags=re.IGNORECASE,
)

HOT_TOKEN_PATTERN = re.compile(r"\b(" + "|".join(HOT_TOKENS) + r")\b", re.IGNORECASE)

CONTEXT_RADIUS = 6


@dataclass(slots=True)
class ReflectionHit:
    """A single reflection usage discovered in a source file."""

    project: str
    rel_path: str
    line_number: int
    snippet: str
    is_hot: bool


def iter_source_files(root: Path) -> Iterable[Path]:
    """Yield ``.cs`` files under ``root`` excluding ``bin``/``obj`` directories."""

    for path in root.rglob("*.cs"):
        if not path.is_file():
            continue
        if any(part.lower() in {"bin", "obj"} for part in path.parts):
            continue
        yield path


def detect_hits(path: Path) -> list[ReflectionHit]:
    """Return all reflection hits for ``path``."""

    try:
        lines = path.read_text(encoding="utf-8").splitlines()
    except UnicodeDecodeError:
        lines = path.read_text(encoding="utf-8", errors="ignore").splitlines()

    text = "\n".join(lines)
    hits: list[ReflectionHit] = []
    for match in REFLECTION_PATTERN.finditer(text):
        line_number = text.count("\n", 0, match.start()) + 1
        lo = max(0, line_number - CONTEXT_RADIUS - 1)
        hi = min(len(lines), line_number + CONTEXT_RADIUS)
        window = "\n".join(lines[lo:hi])
        is_hot = bool(HOT_TOKEN_PATTERN.search(window))
        snippet = lines[line_number - 1].strip()
        rel_path = path.relative_to(REPO_ROOT).as_posix()
        project = rel_path.split("/", 1)[0]
        hits.append(
            ReflectionHit(
                project=project,
                rel_path=rel_path.replace("/", "\\"),
                line_number=line_number,
                snippet=snippet,
                is_hot=is_hot,
            )
        )
    return hits


def group_hits_by_project(hits: list[ReflectionHit], projects: set[str]) -> dict[str, list[ReflectionHit]]:
    """Group hits by top-level project directory respecting the allow-list."""

    grouped: dict[str, list[ReflectionHit]] = {project: [] for project in projects}
    for hit in hits:
        if hit.project in projects:
            grouped.setdefault(hit.project, []).append(hit)
    # Drop empty default entries for projects that were never requested.
    return {project: group for project, group in grouped.items() if group}


def sort_projects(groups: dict[str, list[ReflectionHit]]) -> list[tuple[str, list[ReflectionHit]]]:
    """Sort projects by descending hot-hit count then alphabetically."""

    def sort_key(item: tuple[str, list[ReflectionHit]]) -> tuple[int, str]:
        name, hits = item
        hot_count = sum(hit.is_hot for hit in hits)
        return (-hot_count, name.lower())

    return sorted(groups.items(), key=sort_key)


def format_project_section(name: str, hits: list[ReflectionHit]) -> list[str]:
    """Generate markdown lines for a single project section."""

    sorted_hits = sorted(
        hits,
        key=lambda hit: (
            not hit.is_hot,
            hit.rel_path.lower(),
            hit.line_number,
        ),
    )

    lines: list[str] = [f"## Project: {name}"]
    for rel_path, file_hits in groupby(sorted_hits, key=attrgetter("rel_path")):
        lines.append(f"### {rel_path}")
        for hit in file_hits:
            tag = "**HOT**" if hit.is_hot else "COLD"
            snippet = hit.snippet.replace("|", "\\|")
            lines.append(f"- {tag} @ L{hit.line_number} — `{snippet}`")
        lines.append("")
    lines.append("")
    return lines


def build_todo_contents(project_sections: list[tuple[str, list[ReflectionHit]]]) -> str:
    header_lines = [
        "make todo for this:",
        "- Scan the repo for reflection in hot paths (done by this script).",
        "- Triage: HOT (per-tick/UI) vs COLD (init/config).",
        "- For HOT, replace reflection with cached delegates; keep behavior identical.",
        "- Work in small PR-sized batches; re-scan after each batch.",
        "- Acceptance: zero HOT reflection left; no per-call allocations.",
        "",
        "---",
        "# Task: Replace hot-path reflection with cached delegates (ONI mods)",
        "(See performance brief; scope must stay tight.)",
        "",
        "## Find candidates (current run)",
        "HOT entries first under each project.",
        "",
    ]

    body_lines: list[str] = []
    for name, hits in project_sections:
        body_lines.extend(format_project_section(name, hits))

    if not body_lines:
        body_lines.append("No reflection hotspots were detected in the selected projects.\n")

    return "\n".join(header_lines + body_lines).rstrip("\n") + "\n"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--project",
        dest="projects",
        action="append",
        help="Top-level project directory to include. Can be passed multiple times.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    projects = set(DEFAULT_PROJECTS)
    if args.projects:
        projects.update(args.projects)

    all_hits: list[ReflectionHit] = []
    for path in iter_source_files(REPO_ROOT):
        all_hits.extend(detect_hits(path))

    grouped = group_hits_by_project(all_hits, projects)
    project_sections = sort_projects(grouped)
    TODO_PATH.write_text(build_todo_contents(project_sections), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
