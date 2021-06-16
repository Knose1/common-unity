using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.GitHub.Knose1.Common.XML;

namespace Com.GitHub.Knose1.JuicyText.Attributes
{
	/// <summary>
	/// Create a tag for <see cref="TextEffect"/>.<br/>
	/// Please ensure that a tag is not already used. (There is no warning for that)<br/>
	/// Please ensure that the requested methods exist. (There are warnings and errors for that)
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class TextTagAttribute : Attribute
	{
		private readonly TextTagUsage _textTagUsage;
		private readonly string _tagName;

		/// <param name="textTagUsage">Defines when the tag impact the text (in runtim or when the letter is added)</param>
		/// <param name="tagName"></param>
		public TextTagAttribute(TextTagUsage textTagUsage, string tagName)
		{
			this._textTagUsage = textTagUsage;
			this._tagName = tagName.ToLower();
		}

		public TextTagUsage TextTagUsage => _textTagUsage;
		public string TagName => _tagName;
	}

	
	[Flags]
	public enum TextTagUsage
	{
		/// <summary>
		/// Tag will be played in update.<br/>
		/// <br/>
		/// Define:<br/>
		/// static <see cref="MeshQuad"/> Update(<see cref="int"/> quadIndex, <see cref="MeshQuad"/> quad, <see cref="XMLTag"/> tag, <see cref="TextEffect"/> text)
		/// </summary>
		/// <example>
		/// public static MeshQuad Update(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		/// </example>
		Runtime = 1 << 0,

		/// <summary>
		/// Tag will be played when a letter is added inside the .<br/>
		/// <br/>
		/// Define one of the two methodes :<br/>
		/// static <see cref="void"/> LetterAdded(<see cref="char"/> c, <see cref="int"/> quadIndex, <see cref="XMLTag"/> tag, <see cref="TextEffect"/> text)<br/>
		/// static <see cref="IEnumerator"/> LetterAdded(<see cref="char"/> c, <see cref="int"/> quadIndex, <see cref="XMLTag"/> tag, <see cref="TextEffect"/> text)"
		/// </summary>
		/// <example>
		/// public static void LetterAdded(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		/// public static IEnumerator LetterAdded(int quadIndex, MeshQuad quad, XMLTag tag, TextEffect text)
		/// </example>
		LetterAdding = 1 << 1,
		All = Runtime | LetterAdding
	}
}
