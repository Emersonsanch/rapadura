using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.SkillTree
{
    /// <summary>
    /// Lightweight custom VisualElement that draws straight connector lines between skill
    /// tree nodes using the UI Toolkit vector API (Painter2D), so prerequisite relationships
    /// are visible at a glance without needing separate texture/sprite line assets.
    /// </summary>
    public class SkillTreeConnectorElement : VisualElement
    {
        private readonly List<(Vector2 from, Vector2 to, Color color)> _lines = new List<(Vector2, Vector2, Color)>();

        public SkillTreeConnectorElement()
        {
            generateVisualContent += OnGenerateVisualContent;
            pickingMode = PickingMode.Ignore;
        }

        public void SetLines(IEnumerable<(Vector2 from, Vector2 to, Color color)> lines)
        {
            _lines.Clear();
            _lines.AddRange(lines);
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (_lines.Count == 0)
            {
                return;
            }

            Painter2D painter = context.painter2D;
            painter.lineWidth = 3f;

            foreach ((Vector2 from, Vector2 to, Color color) in _lines)
            {
                painter.strokeColor = color;
                painter.BeginPath();
                painter.MoveTo(from);
                painter.LineTo(to);
                painter.Stroke();
            }
        }
    }
}
