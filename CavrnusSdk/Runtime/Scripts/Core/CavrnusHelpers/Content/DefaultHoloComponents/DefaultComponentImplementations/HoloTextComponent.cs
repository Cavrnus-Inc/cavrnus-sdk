using Collab.Holo;
using UnityBase;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class HoloTextComponent : HoloComponent
	{

		[SerializeField] private TextObjectHoloStreamComponent.TextAlignmentData alignment;
		[SerializeField] private TextObjectHoloStreamComponent.HorizontalOverflowData horizontalOverflow;
		[SerializeField] private TextObjectHoloStreamComponent.VerticalOverflowData verticalOverflow;
		[SerializeField] [Multiline] private string text;
		[SerializeField] private int fontSize;
		[SerializeField] private Color fontColor;
		[SerializeField] private float textSizeWidth;
		[SerializeField] private float textSizeHeight;

		public TextObjectHoloStreamComponent.TextAlignmentData Alignment { get { return alignment; } set { alignment = value; } }
		public TextObjectHoloStreamComponent.HorizontalOverflowData HorizontalOverflow { get { return horizontalOverflow; } set { horizontalOverflow = value; } }
		public TextObjectHoloStreamComponent.VerticalOverflowData VerticalOverflow { get { return verticalOverflow; } set { verticalOverflow = value; } }
		public string Text { get { return text; } set { text = value; } }
		public int FontSize { get { return fontSize; } set { fontSize = value; } }
		public Color FontColor { get { return fontColor; } set { fontColor = value; } }

		public float TextAreaWidth { get { return textSizeWidth; } set { textSizeWidth = value; } }
		public float TextAreaHeight { get { return textSizeHeight; } set { textSizeHeight = value; } }

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<TextObjectHoloStreamComponent>(parent);
			g.Alignment = Alignment;
			g.HorizontalOverflow = HorizontalOverflow;
			g.VerticalOverflow = VerticalOverflow;
			g.Text = Text;
			g.FontSize = FontSize;
			g.FontColor = FontColor.ToColor4F();
			g.TextAreaWidth = TextAreaWidth;
			g.TextAreaHeight = TextAreaHeight;

			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}