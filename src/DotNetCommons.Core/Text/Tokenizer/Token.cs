using System;

namespace DotNetCommons.Core.Text.Tokenizer
{
    public class Token
    {
        public TokenDefinition Definition { get; }
        public TokenList Section { get; } = new TokenList();
        public string Text { get; set; }
        public int Value => Definition.Value;

        public Token(TokenDefinition definition, string text = null)
        {
            Definition = definition;
            Text = text;
        }

        public override string ToString()
        {
            return $"[{Value}:{Text}]";
        }
    }
}
