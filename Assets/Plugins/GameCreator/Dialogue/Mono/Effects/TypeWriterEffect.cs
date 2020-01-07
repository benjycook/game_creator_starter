namespace GameCreator.Dialogue
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TypeWriterEffect
    {
        public enum SymbolType
        {
            Character,
            OpenTag,
            CloseTag
        }

        public class Symbol
        {
            public SymbolType type;
            public string character;
            public int tagIndex;

            public Symbol(SymbolType type, string character)
            {
                this.type = type;
                this.character = character;
            }

            public Symbol(SymbolType type, string character, int tagIndex) : this(type, character)
            {
                this.tagIndex = tagIndex;
            }
        }

        public class Tag
        {
            public Regex openRegex;
            public Regex closeRegex;
            public string close;

            public Tag(Regex openRegex, Regex closeRegex, string close)
            {
                this.openRegex = openRegex;
                this.closeRegex = closeRegex;
                this.close = close;
            }
        }

        private static Tag[] TAGS = new Tag[]
        {
            new Tag(
                new Regex(@"^<b>"), 
                new Regex(@"^</b>"),
                "</b>"
            ),
            new Tag(
                new Regex(@"^<i>"),
                new Regex(@"^</i>"),
                "</i>"
            ),
            new Tag(
                new Regex(@"^<size=[0-9]+>"),
                new Regex(@"^</size>"),
                "</size>"
            ),
            new Tag(
                new Regex(@"^<color=#?[a-zA-Z0-9]+>"),
                new Regex(@"^</color>"),
                "</color>"
            )
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private List<Symbol> symbols;
        private int visibleCharactersCount;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TypeWriterEffect(string message)
        {
            int index = 0;
            this.symbols = new List<Symbol>();

            while (index < message.Length)
            {
                bool matchTag = false;
                for (int i = 0; i < TAGS.Length; ++i)
                {
                    Match openMatch = TAGS[i].openRegex.Match(message.Substring(index, message.Length - index));
                    if (openMatch.Success)
                    {
                        string tag = openMatch.Value;
                        this.symbols.Add(new Symbol(SymbolType.OpenTag, tag, i));

                        index += tag.Length;
                        matchTag = true;
                        break;
                    }

                    Match closeMatch = TAGS[i].closeRegex.Match(message.Substring(index, message.Length - index));
                    if (closeMatch.Success)
                    {
                        string tag = closeMatch.Value;
                        this.symbols.Add(new Symbol(SymbolType.CloseTag, tag, i));

                        index += tag.Length;
                        matchTag = true;
                        break;
                    }
                }

                if (!matchTag)
                {
                    this.symbols.Add(new Symbol(SymbolType.Character, message[index].ToString()));
                    this.visibleCharactersCount++;
                    index++;
                }
            }
        }

        public string GetText(int visibleCharacters)
        {
            #pragma warning disable XS0001
            StringBuilder builder = new StringBuilder();
            #pragma warning restore XS0001

            Stack<int> pendingTags = new Stack<int>();

            for (int i = 0; i < visibleCharacters; ++i)
            {
                if (i >= this.symbols.Count) break;
                builder.Append(this.symbols[i].character);

                switch (this.symbols[i].type)
                {
                    case SymbolType.OpenTag :
                        pendingTags.Push(i);
                        break;

                    case SymbolType.CloseTag:
                        pendingTags.Pop();
                        break;
                }
            }

            while (pendingTags.Count > 0)
            {
                int index = pendingTags.Pop();
                int tagIndex = this.symbols[index].tagIndex;
                builder.Append(TAGS[tagIndex].close);
            }

            return builder.ToString();
        }

        public int CountVisibleCharacters()
        {
            return this.visibleCharactersCount;
        }
    }
}