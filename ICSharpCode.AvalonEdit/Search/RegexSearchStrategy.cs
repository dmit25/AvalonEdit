// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Search
{
    class RegexSearchStrategy : ISearchStrategy
    {
        readonly Regex _searchPattern;
        readonly bool _matchWholeWords;

        public RegexSearchStrategy(string searchPattern, bool matchWholeWords, RegexOptions options)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            _searchPattern = new Regex(searchPattern, options);
            _matchWholeWords = matchWholeWords;
        }

        public IList<ISearchResult> FindAll(string text, int offset, int length, SearchContext context)
        {
            return FindAllMatches(text, offset, length, context, false);
        }

        //static bool IsWordBorder(ITextSource document, int offset)
        //{
        //    return TextUtilities.GetNextCaretPosition(document, offset - 1, LogicalDirection.Forward, CaretPositioningMode.WordBorder) == offset;
        //}

        public ISearchResult FindNext(string text, int offset, int length, SearchContext context)
        {
            var matches = FindAllMatches(text, offset, length, context, true);
            if (matches == null)
            {
                return null;
            }
            return matches.Count > 0 ? matches[0] : null;
        }

        private List<ISearchResult> FindAllMatches(string text, int offset, int length, SearchContext context, bool returnFirst)
        {
            var res = new List<ISearchResult>();
            var endOffset = offset + length;
            return SearchUsingRegex(offset, returnFirst, text, endOffset, res, context);
        }

        private List<ISearchResult> SearchUsingRegex(int offset, bool returnFirst, string text, int endOffset, List<ISearchResult> res, SearchContext context)
        {
            foreach (Match result in _searchPattern.Matches(text))
            {
                if (context.IsCanceled)
                {
                    return null;
                }
                var resultEndOffset = result.Length + result.Index;
                if (offset > result.Index || endOffset < resultEndOffset)
                {
                    continue;
                }
                //if (_matchWholeWords &&
                //    (!IsWordBorder(document, result.Index) || !IsWordBorder(document, resultEndOffset)))
                //{
                //    continue;
                //}
                res.Add(new SearchResult { StartOffset = result.Index, Length = result.Length, Data = result });
                if (returnFirst)
                {
                    break;
                }
            }
            return res;
        }

        public bool Equals(ISearchStrategy other)
        {
            var strategy = other as RegexSearchStrategy;
            return strategy != null &&
                strategy._searchPattern.ToString() == _searchPattern.ToString() &&
                strategy._searchPattern.Options == _searchPattern.Options &&
                strategy._searchPattern.RightToLeft == _searchPattern.RightToLeft;
        }
    }

    class SearchResult : TextSegment, ISearchResult
    {
        public Match Data { get; set; }

        public string ReplaceWith(string replacement)
        {
            if (Data != null)
            {
                return Data.Result(replacement);
            }
            return string.Empty;
        }
    }
}
