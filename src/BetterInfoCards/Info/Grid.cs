using System.Collections.Generic;
using UnityEngine;

namespace BetterInfoCards
{
    public class Grid
    {
        private const float shadowBarSpacing = 4f;

        private readonly List<InfoCardWidgets> cards;
        private readonly List<Column> columns = new();
        private readonly float topY;
        private bool hasPendingCards;
        private bool layoutApplied;
        private bool columnsDirty = true;

        // The HoverTextScreen is initialized before CameraController
        private float _minY = float.MaxValue;
        private float MinY
        {
            get
            {
                if (_minY == float.MaxValue)
                {
                    var canvas = HoverTextScreen.Instance.gameObject.GetComponentInParent<Canvas>();
                    _minY = -canvas.pixelRect.height / (2f * canvas.scaleFactor);
                }

                return _minY;
            }
        }

        public Grid(List<InfoCardWidgets> cards, float topY)
        {
            this.cards = cards ?? new List<InfoCardWidgets>();
            this.topY = topY;

            if (this.cards.Count == 0)
                return;

            var pendingCards = new List<InfoCardWidgets>();

            foreach (var card in this.cards)
            {
                if (card == null)
                    continue;

                var state = card.ResolvePendingWidgets();
                if (state == InfoCardWidgets.PendingShadowBarState.Pending)
                    pendingCards.Add(card);
            }

            hasPendingCards = pendingCards.Count > 0;

            if (hasPendingCards)
                DeferredLayoutScheduler.Register(this, pendingCards);
        }

        public void MoveAndResizeInfoCards()
        {
            ApplyLayoutIfReady();
        }

        private void ApplyLayoutIfReady()
        {
            if (layoutApplied)
                return;

            if (hasPendingCards)
                return;

            if (columnsDirty)
            {
                BuildColumns();
                columnsDirty = false;
            }

            for (int i = columns.Count - 1; i >= 0; i--)
            {
                float colToRightYMin = float.MaxValue;

                if (i != columns.Count - 1)
                    colToRightYMin = columns[i + 1].YMin;

                columns[i].MoveAndResize(colToRightYMin);
            }

            layoutApplied = true;
        }

        private void BuildColumns()
        {
            columns.Clear();

            if (cards.Count == 0)
                return;

            var offset = new Vector2(0f, topY);
            var column = new Column();
            int placedCount = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];

                if (card == null)
                    continue;

                card.ResolvePendingWidgets();

                // If the first one can't fit, put it down anyways otherwise they all get shifted over by the shadow bar spacing.
                if (offset.y - card.Height < MinY + shadowBarSpacing && placedCount > 0)
                {
                    offset.x += column.maxXInCol + shadowBarSpacing;
                    columns.Add(column);
                    column = new Column { offsetX = offset.x };
                    offset.y = topY;
                }

                card.offset.y = offset.y - card.YMax;
                offset.y -= card.Height + shadowBarSpacing;

                if (card.Width > column.maxXInCol)
                    column.maxXInCol = card.Width;

                column.cards.Add(card);
                placedCount++;
            }

            if (column.cards.Count > 0)
                columns.Add(column);
        }

        private void OnPendingCardsResolved()
        {
            hasPendingCards = false;
            columnsDirty = true;
            layoutApplied = false;
            ApplyLayoutIfReady();
        }

        private static class DeferredLayoutScheduler
        {
            private static readonly List<PendingLayout> pendingLayouts = new();
            private static LateUpdateDriver driver;

            public static void Register(Grid grid, List<InfoCardWidgets> pendingCards)
            {
                if (grid == null || pendingCards == null || pendingCards.Count == 0)
                    return;

                var layout = GetOrCreateLayout(grid);
                layout.ReplacePendingCards(pendingCards);

                EnsureDriver()?.Activate();
            }

            private static PendingLayout GetOrCreateLayout(Grid grid)
            {
                for (int i = 0; i < pendingLayouts.Count; i++)
                {
                    var existing = pendingLayouts[i];
                    if (existing.IsFor(grid))
                        return existing;
                }

                var layout = new PendingLayout(grid);
                pendingLayouts.Add(layout);
                return layout;
            }

            private static void Process()
            {
                for (int i = pendingLayouts.Count - 1; i >= 0; i--)
                {
                    var layout = pendingLayouts[i];

                    if (!layout.TryComplete())
                        continue;

                    pendingLayouts.RemoveAt(i);
                }

                if (pendingLayouts.Count == 0 && driver != null)
                    driver.enabled = false;
            }

            private static LateUpdateDriver EnsureDriver()
            {
                if (driver != null)
                    return driver;

                var screen = HoverTextScreen.Instance;
                if (screen == null)
                    return null;

                driver = screen.gameObject.GetComponent<LateUpdateDriver>();
                if (driver == null)
                    driver = screen.gameObject.AddComponent<LateUpdateDriver>();

                return driver;
            }

            private sealed class PendingLayout
            {
                private readonly Grid grid;
                private readonly List<InfoCardWidgets> pendingCards = new();

                public PendingLayout(Grid grid)
                {
                    this.grid = grid;
                }

                public bool IsFor(Grid other)
                {
                    return ReferenceEquals(grid, other);
                }

                public void ReplacePendingCards(List<InfoCardWidgets> cards)
                {
                    pendingCards.Clear();

                    foreach (var card in cards)
                    {
                        if (card == null || pendingCards.Contains(card))
                            continue;

                        pendingCards.Add(card);
                    }
                }

                public bool TryComplete()
                {
                    if (grid == null)
                        return true;

                    if (pendingCards.Count == 0)
                    {
                        grid.OnPendingCardsResolved();
                        return true;
                    }

                    for (int i = pendingCards.Count - 1; i >= 0; i--)
                    {
                        var card = pendingCards[i];

                        if (card == null)
                        {
                            pendingCards.RemoveAt(i);
                            continue;
                        }

                        var state = card.ResolvePendingWidgets();

                        if (state == InfoCardWidgets.PendingShadowBarState.Pending)
                            continue;

                        pendingCards.RemoveAt(i);
                    }

                    if (pendingCards.Count > 0)
                        return false;

                    grid.OnPendingCardsResolved();
                    return true;
                }
            }

            private sealed class LateUpdateDriver : MonoBehaviour
            {
                public void Activate()
                {
                    enabled = true;
                }

                private void OnEnable()
                {
                    if (pendingLayouts.Count == 0)
                        enabled = false;
                }

                private void LateUpdate()
                {
                    Process();
                }
            }
        }
    }
}
