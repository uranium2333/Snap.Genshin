﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Control.Infrastructure.Markdown
{
    public class Markdown : DependencyObject
    {
        /// <summary>
        /// maximum nested depth of [] and () supported by the transform; implementation detail
        /// </summary>
        private const int _nestDepth = 6;

        /// <summary>
        /// Tabs are automatically converted to spaces as part of the transform  
        /// this constant determines how "wide" those tabs become in spaces  
        /// </summary>
        private const int _tabWidth = 4;

        private const string _markerUL = @"[*+-]";
        private const string _markerOL = @"\d+[.]";

        private int _listLevel;

        /// <summary>
        /// when true, bold and italic require non-word characters on either side  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        /// 
        public bool StrictBoldItalic { get; set; }

        public ICommand HyperlinkCommand { get; set; }

        public Style DocumentStyle
        {
            get => (Style)GetValue(DocumentStyleProperty);
            set => SetValue(DocumentStyleProperty, value);
        }

        public Style NormalParagraphStyle
        {
            get => (Style)GetValue(NormalParagraphStyleProperty);
            set => SetValue(NormalParagraphStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for NormalParagraphStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalParagraphStyleProperty =
            DependencyProperty.Register("NormalParagraphStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentStyleProperty =
            DependencyProperty.Register("DocumentStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading1Style
        {
            get => (Style)GetValue(Heading1StyleProperty);
            set => SetValue(Heading1StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading1Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading1StyleProperty =
            DependencyProperty.Register("Heading1Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading2Style
        {
            get => (Style)GetValue(Heading2StyleProperty);
            set => SetValue(Heading2StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading2Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading2StyleProperty =
            DependencyProperty.Register("Heading2Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading3Style
        {
            get => (Style)GetValue(Heading3StyleProperty);
            set => SetValue(Heading3StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading3Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading3StyleProperty =
            DependencyProperty.Register("Heading3Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading4Style
        {
            get => (Style)GetValue(Heading4StyleProperty);
            set => SetValue(Heading4StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading4Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading4StyleProperty =
            DependencyProperty.Register("Heading4Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style CodeStyle
        {
            get => (Style)GetValue(CodeStyleProperty);
            set => SetValue(CodeStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for CodeStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeStyleProperty =
            DependencyProperty.Register("CodeStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style LinkStyle
        {
            get => (Style)GetValue(LinkStyleProperty);
            set => SetValue(LinkStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for LinkStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkStyleProperty =
            DependencyProperty.Register("LinkStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ImageStyle
        {
            get => (Style)GetValue(ImageStyleProperty);
            set => SetValue(ImageStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageStyleProperty =
            DependencyProperty.Register("ImageStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style SeparatorStyle
        {
            get => (Style)GetValue(SeparatorStyleProperty);
            set => SetValue(SeparatorStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for SeparatorStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeparatorStyleProperty =
            DependencyProperty.Register("SeparatorStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public string AssetPathRoot
        {
            get => (string)GetValue(AssetPathRootProperty);
            set => SetValue(AssetPathRootProperty, value);
        }

        public static readonly DependencyProperty AssetPathRootProperty =
            DependencyProperty.Register("AssetPathRootRoot", typeof(string), typeof(Markdown), new PropertyMetadata(null));

        public Style TableStyle
        {
            get => (Style)GetValue(TableStyleProperty);
            set => SetValue(TableStyleProperty, value);
        }

        public static readonly DependencyProperty TableStyleProperty =
            DependencyProperty.Register("TableStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableHeaderStyle
        {
            get => (Style)GetValue(TableHeaderStyleProperty);
            set => SetValue(TableHeaderStyleProperty, value);
        }

        public static readonly DependencyProperty TableHeaderStyleProperty =
            DependencyProperty.Register("TableHeaderStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableBodyStyle
        {
            get => (Style)GetValue(TableBodyStyleProperty);
            set => SetValue(TableBodyStyleProperty, value);
        }

        public static readonly DependencyProperty TableBodyStyleProperty =
            DependencyProperty.Register("TableBodyStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Markdown()
        {
            HyperlinkCommand = NavigationCommands.GoToPage;
        }

        public FlowDocument Transform(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            text = Normalize(text);
            FlowDocument document = Create<FlowDocument, Block>(RunBlockGamut(text));

            if (DocumentStyle != null)
            {
                document.Style = DocumentStyle;
            }
            else
            {
                document.PagePadding = new Thickness(0);
            }

            return document;
        }

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Block> RunBlockGamut(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return DoHeaders(text,
                s1 => DoHorizontalRules(s1,
                    s2 => DoLists(s2,
                    s3 => DoTable(s3,
                    sn => FormParagraphs(sn)))));
        }

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Inline> RunSpanGamut(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return DoCodeSpans(text,
                s0 => DoImages(s0,
                s1 => DoAnchors(s1,
                s2 => DoItalicsAndBold(s2,
                s3 => DoText(s3)))));
        }

        private static readonly Regex _newlinesLeadingTrailing = new(@"^\n+|\n+\z", RegexOptions.Compiled);
        private static readonly Regex _newlinesMultiple = new(@"\n{2,}", RegexOptions.Compiled);

        /// <summary>
        /// splits on two or more newlines, to form "paragraphs";    
        /// </summary>
        private IEnumerable<Block> FormParagraphs(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // split on two or more newlines
            string[] grafs = _newlinesMultiple.Split(_newlinesLeadingTrailing.Replace(text, ""));

            foreach (string g in grafs)
            {
                Paragraph block = Create<Paragraph, Inline>(RunSpanGamut(g));
                block.Style = NormalParagraphStyle;
                yield return block;
            }
        }

        private static string? _nestedBracketsPattern;

        /// <summary>
        /// Reusable pattern to match balanced [brackets]. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to _nestDepth
            if (_nestedBracketsPattern is null)
            {
                _nestedBracketsPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^\[\]]+      # Anything other than brackets
                     |
                       \[
                           ", _nestDepth) + RepeatString(
                    @" \]
                    )*"
                    , _nestDepth);
            }

            return _nestedBracketsPattern;
        }

        private static string? _nestedParensPattern;

        /// <summary>
        /// Reusable pattern to match balanced (parens). See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPattern is null)
            {
                _nestedParensPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()\s]+      # Anything other than parens or whitespace
                     |
                       \(
                           ", _nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , _nestDepth);
            }

            return _nestedParensPattern;
        }

        private static string? _nestedParensPatternWithWhiteSpace;

        /// <summary>
        /// Reusable pattern to match balanced (parens), including whitespace. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPatternWithWhiteSpace()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPatternWithWhiteSpace is null)
            {
                _nestedParensPatternWithWhiteSpace =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()]+      # Anything other than parens
                     |
                       \(
                           ", _nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , _nestDepth);
            }

            return _nestedParensPatternWithWhiteSpace;
        }

        private static readonly Regex _imageInline = new(
            string.Format(CultureInfo.InvariantCulture, @"
                (                           # wrap whole match in $1
                    !\[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href = $3
                        [ ]*
                        (                   # $4
                        (['""])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        #[ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )",
            GetNestedBracketsPattern(),
            GetNestedParensPatternWithWhiteSpace()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _anchorInline = new(
            string.Format(CultureInfo.InvariantCulture, @"
                (                           # wrap whole match in $1
                    \[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href = $3
                        [ ]*
                        (                   # $4
                        (['""])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPattern()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown images into images
        /// </summary>
        /// <remarks>
        /// ![image alt](url) 
        /// </remarks>
        private IEnumerable<Inline> DoImages(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _imageInline, ImageInlineEvaluator, defaultHandler);
        }

        private Inline ImageInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            BitmapImage? imgSource = null;
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !System.IO.Path.IsPathRooted(url))
                {
                    url = System.IO.Path.Combine(AssetPathRoot ?? string.Empty, url);
                }

                imgSource = new BitmapImage();
                imgSource.BeginInit();
                imgSource.CacheOption = BitmapCacheOption.None;
                imgSource.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                imgSource.CacheOption = BitmapCacheOption.OnLoad;
                imgSource.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                imgSource.UriSource = new Uri(url);
                imgSource.EndInit();
            }
            catch (Exception)
            {
                return new Run("!" + url) { Foreground = Brushes.Red };
            }

            Image image = new() { Source = imgSource, Tag = linkText };
            if (ImageStyle is null)
            {
                image.Margin = new Thickness(0);
            }
            else
            {
                image.Style = ImageStyle;
            }

            // Bind size so document is updated when image is downloaded
            if (imgSource.IsDownloading)
            {
                Binding binding = new(nameof(BitmapImage.Width))
                {
                    Source = imgSource,
                    Mode = BindingMode.OneWay
                };

                BindingExpressionBase bindingExpression = BindingOperations.SetBinding(image, FrameworkElement.WidthProperty, binding);

                void downloadCompletedHandler(object? sender, EventArgs e)
                {
                    imgSource.DownloadCompleted -= downloadCompletedHandler;
                    bindingExpression.UpdateTarget();
                }

                imgSource.DownloadCompleted += downloadCompletedHandler;
            }
            else
            {
                image.Width = imgSource.Width;
            }

            return new InlineUIContainer(image);
        }

        /// <summary>
        /// Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        /// [link text](url "title") 
        /// </remarks>
        private IEnumerable<Inline> DoAnchors(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // Next, inline-style links: [link text](url "optional title") or [link text](url "optional title")
            return Evaluate(text, _anchorInline, AnchorInlineEvaluator, defaultHandler);
        }

        private Inline AnchorInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = match.Groups[6].Value;

            Hyperlink result = Create<Hyperlink, Inline>(RunSpanGamut(linkText));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;
            if (!string.IsNullOrWhiteSpace(title))
            {
                result.ToolTip = title;
            }

            if (LinkStyle != null)
            {
                result.Style = LinkStyle;
            }

            return result;
        }

        private static readonly Regex _headerSetext = new(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+",
    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _headerAtx = new(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \#*         # optional closing #'s (not counted)
                \n+",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// 
        /// # Header 1  
        /// ## Header 2  
        /// ## Header 2 with closing hashes ##  
        /// ...  
        /// ###### Header 6  
        /// </remarks>
        private IEnumerable<Block> DoHeaders(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _headerSetext, m => SetextHeaderEvaluator(m),
                s => Evaluate(s, _headerAtx, m => AtxHeaderEvaluator(m), defaultHandler));
        }

        private Block SetextHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string header = match.Groups[1].Value;
            int level = match.Groups[2].Value.StartsWith("=", StringComparison.Ordinal) ? 1 : 2;

            //TODO: Style the paragraph based on the header level
            return CreateHeader(level, RunSpanGamut(header.Trim()));
        }

        private Block AtxHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return CreateHeader(level, RunSpanGamut(header));
        }

        public Block CreateHeader(int level, IEnumerable<Inline> content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Paragraph block = Create<Paragraph, Inline>(content);

            switch (level)
            {
                case 1:
                    if (Heading1Style != null)
                    {
                        block.Style = Heading1Style;
                    }

                    break;

                case 2:
                    if (Heading2Style != null)
                    {
                        block.Style = Heading2Style;
                    }

                    break;

                case 3:
                    if (Heading3Style != null)
                    {
                        block.Style = Heading3Style;
                    }

                    break;

                case 4:
                    if (Heading4Style != null)
                    {
                        block.Style = Heading4Style;
                    }

                    break;
            }

            return block;
        }

        private static readonly Regex _horizontalRules = new(@"
            ^[ ]{0,3}         # Leading space
                ([-*_])       # $1: First marker
                (?>           # Repeated marker group
                    [ ]{0,2}  # Zero, one, or two spaces.
                    \1        # Marker character
                ){2,}         # Group repeated at least twice
                [ ]*          # Trailing spaces
                $             # End of line.
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        /// <remarks>
        /// ***  
        /// * * *  
        /// ---
        /// - - -
        /// </remarks>
        private IEnumerable<Block> DoHorizontalRules(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _horizontalRules, RuleEvaluator, defaultHandler);
        }

        private Block RuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            Separator separator = new();
            if (SeparatorStyle != null)
            {
                separator.Style = SeparatorStyle;
            }

            BlockUIContainer container = new(separator);
            return container;
        }

        private static readonly string _wholeList
            = string.Format(CultureInfo.InvariantCulture, @"
            (                               # $1 = whole list
              (                             # $2
                [ ]{{0,{1}}}
                ({0})                       # $3 = first list item marker
                [ ]+
              )
              (?s:.+?)
              (                             # $4
                  \z
                |
                  \n{{2,}}
                  (?=\S)
                  (?!                       # Negative lookahead for another list item marker
                    [ ]*
                    {0}[ ]+
                  )
              )
            )", string.Format(CultureInfo.InvariantCulture, "(?:{0}|{1})", _markerUL, _markerOL), _tabWidth - 1);

        private static readonly Regex _listNested = new(@"^" + _wholeList, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _listTopLevel = new(@"(?:(?<=\n\n)|\A\n?)" + _wholeList, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private IEnumerable<Block> DoLists(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // We use a different prefix before nested lists than top-level lists.
            // See extended comment in _ProcessListItems().
            if (_listLevel > 0)
            {
                return Evaluate(text, _listNested, ListEvaluator, defaultHandler);
            }
            else
            {
                return Evaluate(text, _listTopLevel, ListEvaluator, defaultHandler);
            }
        }

        private Block ListEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string list = match.Groups[1].Value;
            string listType = Regex.IsMatch(match.Groups[3].Value, _markerUL) ? "ul" : "ol";

            // Turn double returns into triple returns, so that we can make a
            // paragraph for the last item in a list, if necessary:
            list = Regex.Replace(list, @"\n{2,}", "\n\n\n");

            List resultList = Create<List, ListItem>(ProcessListItems(list, listType == "ul" ? _markerUL : _markerOL));

            resultList.MarkerStyle = listType == "ul" ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal;

            return resultList;
        }

        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        private IEnumerable<ListItem> ProcessListItems(string list, string marker)
        {
            _listLevel++;
            try
            {
                // Trim trailing blank lines:
                list = Regex.Replace(list, @"\n{2,}\z", "\n");

                string pattern = string.Format(CultureInfo.InvariantCulture,
                  @"(\n)?                      # leading line = $1
                (^[ ]*)                    # leading whitespace = $2
                ({0}) [ ]+                 # list marker = $3
                ((?s:.+?)                  # list item text = $4
                (\n{{1,2}}))      
                (?= \n* (\z | \2 ({0}) [ ]+))", marker);

                Regex regex = new(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                MatchCollection matches = regex.Matches(list);
                foreach (Match m in matches)
                {
                    yield return ListItemEvaluator(m);
                }
            }
            finally
            {
                _listLevel--;
            }
        }

        private ListItem ListItemEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string item = match.Groups[4].Value;
            string leadingLine = match.Groups[1].Value;

            if (!string.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
            {
                // we could correct any bad indentation here..
                return Create<ListItem, Block>(RunBlockGamut(item));
            }
            else
            {
                // recursion for sub-lists
                return Create<ListItem, Block>(RunBlockGamut(item));
            }
        }

        private static readonly Regex _table = new(@"
            (                               # $1 = whole table
                [ \r\n]*
                (                           # $2 = table header
                    \|([^|\r\n]*\|)+        # $3
                )
                [ ]*\r?\n[ ]*
                (                           # $4 = column style
                    \|(:?-+:?\|)+           # $5
                )
                (                           # $6 = table row
                    (                       # $7
                        [ ]*\r?\n[ ]*
                        \|([^|\r\n]*\|)+    # $8
                    )+
                )
            )", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public IEnumerable<Block> DoTable(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, _table, TableEvalutor, defaultHandler);
        }

        private Block TableEvalutor(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string wholeTable = match.Groups[1].Value;
            string header = match.Groups[2].Value.Trim();
            string style = match.Groups[4].Value.Trim();
            string row = match.Groups[6].Value.Trim();

            string[] styles = style[1..^1].Split('|');
            string[] headers = header[1..^1].Split('|');
            List<string[]> rowList = row.Split('\n').Select(ritm =>
            {
                string trimRitm = ritm.Trim();
                return trimRitm[1..^1].Split('|');
            }).ToList();

            int maxColCount =
                Math.Max(
                    Math.Max(styles.Length, headers.Length),
                    rowList.Select(ritm => ritm.Length).Max()
                );


            // table style
            List<TextAlignment?> aligns = new();
            foreach (string colStyleTxt in styles)
            {
                char firstChar = colStyleTxt.First();
                char lastChar = colStyleTxt.Last();
                // center
                if (firstChar == ':' && lastChar == ':')
                {
                    aligns.Add(TextAlignment.Center);
                }
                // right
                else if (lastChar == ':')
                {
                    aligns.Add(TextAlignment.Right);
                }
                // left
                else if (firstChar == ':')
                {
                    aligns.Add(TextAlignment.Left);
                }
                // default
                else
                {
                    aligns.Add(null);
                }
            }
            while (aligns.Count < maxColCount)
            {
                aligns.Add(null);
            }

            // table
            Table table = new();
            if (TableStyle != null)
            {
                table.Style = TableStyle;
            }

            // table columns
            while (table.Columns.Count < maxColCount)
            {
                table.Columns.Add(new TableColumn());
            }

            // table header
            TableRowGroup tableHeaderRG = new();
            if (TableHeaderStyle != null)
            {
                tableHeaderRG.Style = TableHeaderStyle;
            }

            TableRow tableHeader = CreateTableRow(headers, aligns);
            tableHeaderRG.Rows.Add(tableHeader);
            table.RowGroups.Add(tableHeaderRG);

            // row
            TableRowGroup tableBodyRG = new();
            if (TableBodyStyle != null)
            {
                tableBodyRG.Style = TableBodyStyle;
            }

            foreach (string[] rowAry in rowList)
            {
                TableRow tableBody = CreateTableRow(rowAry, aligns);
                tableBodyRG.Rows.Add(tableBody);
            }
            table.RowGroups.Add(tableBodyRG);

            return table;
        }

        private TableRow CreateTableRow(string[] txts, List<TextAlignment?> aligns)
        {
            TableRow tableRow = new();

            foreach (int idx in Enumerable.Range(0, txts.Length))
            {
                string txt = txts[idx];
                TextAlignment? align = aligns[idx];

                Paragraph paragraph = Create<Paragraph, Inline>(RunSpanGamut(txt));
                TableCell cell = new(paragraph);

                if (align.HasValue)
                {
                    cell.TextAlignment = align.Value;
                }

                tableRow.Cells.Add(cell);
            }

            while (tableRow.Cells.Count < aligns.Count)
            {
                tableRow.Cells.Add(new TableCell());
            }

            return tableRow;
        }

        private static readonly Regex _codeSpan = new(@"
                    (?<!\\)   # Character before opening ` can't be a backslash
                    (`+)      # $1 = Opening run of `
                    (.+?)     # $2 = The code block
                    (?<!`)
                    \1
                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown `code spans` into HTML code tags
        /// </summary>
        private IEnumerable<Inline> DoCodeSpans(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            //    * You can use multiple backticks as the delimiters if you want to
            //        include literal backticks in the code span. So, this input:
            //
            //        Just type ``foo `bar` baz`` at the prompt.
            //
            //        Will translate to:
            //
            //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
            //
            //        There's no arbitrary limit to the number of backticks you
            //        can use as delimters. If you need three consecutive backticks
            //        in your code, use four for delimiters, etc.
            //
            //    * You can use spaces to get literal backticks at the edges:
            //
            //          ... type `` `bar` `` ...
            //
            //        Turns to:
            //
            //          ... type <code>`bar`</code> ...         
            //

            return Evaluate(text, _codeSpan, CodeSpanEvaluator, defaultHandler);
        }

        private Inline CodeSpanEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            Run result = new(span);
            if (CodeStyle != null)
            {
                result.Style = CodeStyle;
            }

            return result;
        }

        private static readonly Regex _bold = new(@"(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _strictBold = new(@"([\W_]|^) (\*\*|__) (?=\S) ([^\r]*?\S[\*_]*) \2 ([\W_]|$)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex _italic = new(@"(\*|_) (?=\S) (.+?) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _strictItalic = new(@"([\W_]|^) (\*|_) (?=\S) ([^\r\*_]*?\S) \2 ([\W_]|$)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        private IEnumerable<Inline> DoItalicsAndBold(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // <strong> must go first, then <em>
            if (StrictBoldItalic)
            {
                return Evaluate(text, _strictBold, m => BoldEvaluator(m, 3),
                    s1 => Evaluate(s1, _strictItalic, m => ItalicEvaluator(m, 3),
                    s2 => defaultHandler(s2)));
            }
            else
            {
                return Evaluate(text, _bold, m => BoldEvaluator(m, 2),
                   s1 => Evaluate(s1, _italic, m => ItalicEvaluator(m, 2),
                   s2 => defaultHandler(s2)));
            }
        }

        private Inline ItalicEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string content = match.Groups[contentGroup].Value;
            return Create<Italic, Inline>(RunSpanGamut(content));
        }

        private Inline BoldEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            string content = match.Groups[contentGroup].Value;
            return Create<Bold, Inline>(RunSpanGamut(content));
        }

        private static readonly Regex _outDent = new(@"^[ ]{1," + _tabWidth + @"}", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Remove one level of line-leading spaces
        /// </summary>
        private string Outdent(string block)
        {
            return _outDent.Replace(block, "");
        }

        /// <summary>
        /// convert all tabs to _tabWidth spaces; 
        /// standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
        /// makes sure text ends with a couple of newlines; 
        /// removes any blank lines (only spaces) in the text
        /// </summary>
        private string Normalize(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            StringBuilder output = new(text.Length);
            StringBuilder line = new();
            bool valid = false;

            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        if (valid)
                        {
                            output.Append(line);
                        }

                        output.Append('\n');
                        line.Length = 0;
                        valid = false;
                        break;
                    case '\r':
                        if (i < text.Length - 1 && text[i + 1] != '\n')
                        {
                            if (valid)
                            {
                                output.Append(line);
                            }

                            output.Append('\n');
                            line.Length = 0;
                            valid = false;
                        }
                        break;
                    case '\t':
                        int width = _tabWidth - line.Length % _tabWidth;
                        for (int k = 0; k < width; k++)
                        {
                            line.Append(' ');
                        }

                        break;
                    case '\x1A':
                        break;
                    default:
                        if (!valid && text[i] != ' ')
                        {
                            valid = true;
                        }

                        line.Append(text[i]);
                        break;
                }
            }

            if (valid)
            {
                output.Append(line);
            }

            output.Append('\n');

            // add two newlines to the end before return
            return output.Append("\n\n").ToString();
        }

        /// <summary>
        /// this is to emulate what's available in PHP
        /// </summary>
        private static string RepeatString(string value, int count)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            StringBuilder sb = new(value.Length * count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        private TResult Create<TResult, TContent>(IEnumerable<TContent> content)
            where TResult : IAddChild, new()
        {
            TResult result = new();
            foreach (TContent c in content)
            {
                result.AddChild(c);
            }

            return result;
        }

        private IEnumerable<T> Evaluate<T>(string text, Regex expression, Func<Match, T> build, Func<string, IEnumerable<T>> rest)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            MatchCollection matches = expression.Matches(text);
            int index = 0;
            foreach (Match m in matches)
            {
                if (m.Index > index)
                {
                    string prefix = text[index..m.Index];
                    foreach (T t in rest(prefix))
                    {
                        yield return t;
                    }
                }

                yield return build(m);

                index = m.Index + m.Length;
            }

            if (index < text.Length)
            {
                string suffix = text[index..];
                foreach (T t in rest(suffix))
                {
                    yield return t;
                }
            }
        }

        private static readonly Regex _eoln = new("\\s+");
        private static readonly Regex _lbrk = new(@"\ {2,}\n");

        public IEnumerable<Inline> DoText(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            string[] lines = _lbrk.Split(text);
            bool first = true;
            foreach (string line in lines)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    yield return new LineBreak();
                }

                string t = _eoln.Replace(line, " ");
                yield return new Run(t);
            }
        }
    }
}
