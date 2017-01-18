using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;
using CommonMark.Syntax;

namespace NuSysApp.Components.NuSysRenderer.UI.Textbox.Markdown
{
    public class CustomHtmlFormatter : CommonMark.Formatters.HtmlFormatter
    {
        /// <summary>
        /// This class converts markdown to html
        /// </summary>
        /// <param name="target"></param>
        /// <param name="settings"></param>
        public CustomHtmlFormatter(System.IO.TextWriter target, CommonMarkSettings settings) : base(target, settings)
        {
        }

        /// <summary>
        /// Writes the specified block element to the output stream. Does not write the child nodes, instead
        /// the <paramref name="ignoreChildNodes"/> is used to notify the caller whether it should recurse
        /// into the child nodes.
        /// </summary>
        /// <param name="block">The block element to be written to the output stream.</param>
        /// <param name="isOpening">Specifies whether the block element is being opened (or started).</param>
        /// <param name="isClosing">Specifies whether the block element is being closed. If the block does not
        /// have child nodes, then both <paramref name="isClosing"/> and <paramref name="isOpening"/> can be
        /// <see langword="true"/> at the same time.</param>
        /// <param name="ignoreChildNodes">Instructs the caller whether to skip processing of child nodes or not.</param>
        protected virtual void WriteBlock(Block block, bool isOpening, bool isClosing, out bool ignoreChildNodes)
        {
            ignoreChildNodes = false;
            int x;

            switch (block.Tag)
            {
                case BlockTag.Document:
                    break;

                case BlockTag.Paragraph:
                    if (RenderTightParagraphs.Peek())
                        break;

                    if (isOpening)
                    {
                        EnsureNewLine();
                        Write("<p");
                        if (Settings.TrackSourcePosition) WritePositionAttribute(block);
                        Write('>');
                    }

                    if (isClosing)
                        WriteLine("</p>");

                    break;

                case BlockTag.BlockQuote:
                    if (isOpening)
                    {
                        EnsureNewLine();
                        Write("<blockquote");
                        if (Settings.TrackSourcePosition) WritePositionAttribute(block);
                        WriteLine(">");

                        RenderTightParagraphs.Push(false);
                    }

                    if (isClosing)
                    {
                        RenderTightParagraphs.Pop();
                        WriteLine("</blockquote>");
                    }

                    break;

                case BlockTag.ListItem:
                    if (isOpening)
                    {
                        EnsureNewLine();
                        Write("<li");
                        if (Settings.TrackSourcePosition) WritePositionAttribute(block);
                        Write('>');
                    }

                    if (isClosing)
                        WriteLine("</li>");

                    break;

                case BlockTag.List:
                    var data = block.ListData;

                    if (isOpening)
                    {
                        EnsureNewLine();
                        Write(data.ListType == ListType.Bullet ? "<ul" : "<ol");
                        if (data.Start != 1)
                        {
                            Write(" start=\"");
                            Write(data.Start.ToString(CultureInfo.InvariantCulture));
                            Write('\"');
                        }
                        if (Settings.TrackSourcePosition) WritePositionAttribute(block);
                        WriteLine(">");

                        RenderTightParagraphs.Push(data.IsTight);
                    }

                    if (isClosing)
                    {
                        WriteLine(data.ListType == ListType.Bullet ? "</ul>" : "</ol>");
                        RenderTightParagraphs.Pop();
                    }

                    break;

                case BlockTag.AtxHeading:
                case BlockTag.SetextHeading:

                    x = block.Heading.Level;
                    if (isOpening)
                    {
                        EnsureNewLine();

                        Write("<h" + x.ToString(CultureInfo.InvariantCulture));
                        if (Settings.TrackSourcePosition)
                            WritePositionAttribute(block);

                        Write('>');
                    }

                    if (isClosing)
                        WriteLine("</h" + x.ToString(CultureInfo.InvariantCulture) + ">");

                    break;

                case BlockTag.IndentedCode:
                case BlockTag.FencedCode:

                    ignoreChildNodes = true;

                    EnsureNewLine();
                    Write("<pre><code");
                    if (Settings.TrackSourcePosition) WritePositionAttribute(block);

                    var info = block.FencedCodeData == null ? null : block.FencedCodeData.Info;
                    if (info != null && info.Length > 0)
                    {
                        x = info.IndexOf(' ');
                        if (x == -1)
                            x = info.Length;

                        Write(" class=\"language-");
                        WriteEncodedHtml(info.Substring(0, x));
                        Write('\"');
                    }
                    Write('>');
                    WriteEncodedHtml(block.StringContent);
                    WriteLine("</code></pre>");
                    break;

                case BlockTag.HtmlBlock:
                    ignoreChildNodes = true;
                    // cannot output source position for HTML blocks
                    Write(block.StringContent);

                    break;

                case BlockTag.ThematicBreak:
                    ignoreChildNodes = true;
                    if (Settings.TrackSourcePosition)
                    {
                        Write("<hr");
                        WritePositionAttribute(block);
                        WriteLine();
                    }
                    else
                    {
                        WriteLine("<hr />");
                    }

                    break;

                case BlockTag.ReferenceDefinition:
                    break;

                default:
                    throw new CommonMarkException("Block type " + block.Tag + " is not supported.", block);
            }

            if (ignoreChildNodes && !isClosing)
                throw new InvalidOperationException("Block of type " + block.Tag + " cannot contain child nodes.");
        }

        /// <summary>
        /// Writes the specified inline element to the output stream. Does not write the child nodes, instead
        /// the <paramref name="ignoreChildNodes"/> is used to notify the caller whether it should recurse
        /// into the child nodes.
        /// </summary>
        /// <param name="inline">The inline element to be written to the output stream.</param>
        /// <param name="isOpening">Specifies whether the inline element is being opened (or started).</param>
        /// <param name="isClosing">Specifies whether the inline element is being closed. If the inline does not
        /// have child nodes, then both <paramref name="isClosing"/> and <paramref name="isOpening"/> can be
        /// <see langword="true"/> at the same time.</param>
        /// <param name="ignoreChildNodes">Instructs the caller whether to skip processing of child nodes or not.</param>
        protected override void WriteInline(Inline inline, bool isOpening, bool isClosing, out bool ignoreChildNodes)
        {
            if (RenderPlainTextInlines.Peek())
            {
                switch (inline.Tag)
                {
                    case InlineTag.String:
                    case InlineTag.Code:
                    case InlineTag.RawHtml:
                        WriteEncodedHtml(inline.LiteralContent);
                        break;

                    case InlineTag.LineBreak:
                    case InlineTag.SoftBreak:
                        WriteLine();
                        break;

                    case InlineTag.Image:
                        if (isOpening)
                            RenderPlainTextInlines.Push(true);

                        if (isClosing)
                        {
                            RenderPlainTextInlines.Pop();

                            if (!RenderPlainTextInlines.Peek())
                                goto useFullRendering;
                        }

                        break;

                    case InlineTag.Link:
                    case InlineTag.Strong:
                    case InlineTag.Emphasis:
                    case InlineTag.Strikethrough:
                    case InlineTag.Placeholder:
                        break;

                    default:
                        throw new CommonMarkException("Inline type " + inline.Tag + " is not supported.", inline);
                }

                ignoreChildNodes = false;
                return;
            }

            useFullRendering:

            switch (inline.Tag)
            {
                case InlineTag.String:
                    ignoreChildNodes = true;
                    if (Settings.TrackSourcePosition)
                    {
                        Write("<span");
                        WritePositionAttribute(inline);
                        Write('>');
                        WriteEncodedHtml(inline.LiteralContent ?? string.Empty);
                        Write("</span>");
                    }
                    else
                    {
                        WriteEncodedHtml(inline.LiteralContent ?? string.Empty);
                    }

                    break;

                case InlineTag.LineBreak:
                    ignoreChildNodes = true;
                    WriteLine("<br />");
                    break;

                case InlineTag.SoftBreak:
                    ignoreChildNodes = true;
                    if (Settings.RenderSoftLineBreaksAsLineBreaks)
                        WriteLine("<br />");
                    else
                        WriteLine();
                    break;

                case InlineTag.Code:
                    ignoreChildNodes = true;
                    Write("<code");
                    if (Settings.TrackSourcePosition) WritePositionAttribute(inline);
                    Write('>');
                    WriteEncodedHtml(inline.LiteralContent ?? string.Empty);
                    Write("</code>");
                    break;

                case InlineTag.RawHtml:
                    ignoreChildNodes = true;
                    // cannot output source position for HTML blocks
                    Write(inline.LiteralContent ?? string.Empty);
                    break;

                case InlineTag.Link:
                    ignoreChildNodes = false;

                    if (isOpening)
                    {
                        Write("<a href=\"");
                        var uriResolver = Settings.UriResolver;
                        if (uriResolver != null)
                            WriteEncodedUrl(uriResolver(inline.TargetUrl));
                        else
                            WriteEncodedUrl(inline.TargetUrl);

                        Write('\"');
                        if ((inline.LiteralContent ?? string.Empty).Length > 0)
                        {
                            Write(" title=\"");
                            WriteEncodedHtml(inline.LiteralContent ?? string.Empty);
                            Write('\"');
                        }

                        if (Settings.TrackSourcePosition) WritePositionAttribute(inline);

                        Write('>');
                    }

                    if (isClosing)
                    {
                        Write("</a>");
                    }

                    break;

                case InlineTag.Image:
                    ignoreChildNodes = false;

                    if (isOpening)
                    {
                        Write("<img src=\"");
                        var uriResolver = Settings.UriResolver;
                        if (uriResolver != null)
                            WriteEncodedUrl(uriResolver(inline.TargetUrl));
                        else
                            WriteEncodedUrl(inline.TargetUrl);

                        Write("\" alt=\"");

                        if (!isClosing)
                            RenderPlainTextInlines.Push(true);
                    }

                    if (isClosing)
                    {
                        // this.RenderPlainTextInlines.Pop() is done by the plain text renderer above.

                        Write('\"');
                        if ((inline.LiteralContent ?? string.Empty).Length > 0)
                        {
                            Write(" title=\"");
                            WriteEncodedHtml(inline.LiteralContent ?? string.Empty);
                            Write('\"');
                        }

                        if (Settings.TrackSourcePosition) WritePositionAttribute(inline);
                        Write(" />");
                    }

                    break;

                case InlineTag.Strong:
                    ignoreChildNodes = false;

                    if (isOpening)
                    {
                        // underline support
                        if (inline.Emphasis.DelimiterCharacter == '_')
                        {
                            Write("<u");
                        }
                        else
                        {
                            Write("<strong");
                            if (Settings.TrackSourcePosition) WritePositionAttribute(inline);
                        }

                        Write('>');
                    }

                    if (isClosing)
                    {
                        // underline support
                        Write(inline.Emphasis.DelimiterCharacter == '_' ? "</u>" : "</strong>");
                    }
                    break;

                case InlineTag.Emphasis:
                    ignoreChildNodes = false;

                    if (isOpening)
                    {
                        // underline support
                        if (inline.Emphasis.DelimiterCharacter == '_')
                        {
                            Write("<u");
                        }
                        else
                        {
                            Write("<em");
                            if (Settings.TrackSourcePosition) WritePositionAttribute(inline);
                        }
                        Write('>');
                    }

                    if (isClosing)
                    {
                        // underline support
                        Write(inline.Emphasis.DelimiterCharacter == '_' ? "</u>" : "</em>");
                    }
                    break;

                case InlineTag.Strikethrough:
                    ignoreChildNodes = false;

                    if (isOpening)
                    {
                        Write("<del");
                        if (Settings.TrackSourcePosition) WritePositionAttribute(inline);
                        Write('>');
                    }

                    if (isClosing)
                    {
                        Write("</del>");
                    }
                    break;

                case InlineTag.Placeholder: // placeholders are things like in django {user.name}  or {today.date}
                    base.WriteInline(inline, isOpening, isClosing, out ignoreChildNodes);
                    break;
                default:
                    throw new CommonMarkException("Inline type " + inline.Tag + " is not supported.", inline);
            }
        }


    }
}
