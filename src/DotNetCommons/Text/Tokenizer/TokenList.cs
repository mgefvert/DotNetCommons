using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetCommons.Collections;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer
{
    public class TokenList : List<Token>
    {
        public TokenList()
        {
        }

        public TokenList(IEnumerable<Token> tokens)
        {
            AddRange(tokens);
        }

        /// <summary>
        /// Consume one token of a particular kind, throwing an exception if an illegal token was found.
        /// </summary>
        /// <param name="allowed">Allowed tokens.</param>
        /// <returns>A token, or null if at the end.</returns>
        public Token Consume(params int[] allowed)
        {
            var result = this.ExtractFirstOrDefault();
            if (result != null && allowed != null && allowed.Length > 0 && !allowed.Contains(result.Value))
                throw new StringTokenizerException($"Illegal token '{result.Text}' in text.");

            return result;
        }

        /// <summary>
        /// Consume allowed tokens until the end of the string, throwing an exception if an illegal token was found.
        /// </summary>
        /// <param name="allowed">Allowed tokens.</param>
        /// <returns>An enumeration of tokens.</returns>
        public IEnumerable<Token> ConsumeAll(params int[] allowed)
        {
            while (Count > 0)
                yield return Consume(allowed);
        }

        /// <summary>
        /// Consume tokens until a stop token was found. The stop token will be left on the stream.
        /// </summary>
        /// <param name="stop">Stop tokens.</param>
        /// <returns>An enumeration of tokens.</returns>
        public IEnumerable<Token> ConsumeUntil(params int[] stop)
        {
            while (Count > 0 && !Peek(stop))
                yield return this.ExtractFirst();
        }

        /// <summary>
        /// Peek at the next token.
        /// </summary>
        /// <returns></returns>
        public Token Peek()
        {
            return this.FirstOrDefault();
        }

        /// <summary>
        /// Peek at the next token and see if it's of a particular kind.
        /// </summary>
        /// <returns>True if the next token matches the allowed token list.</returns>
        public bool Peek(params int[] allowed)
        {
            var next = Peek();
            return next != null && allowed.Contains(next.Value);
        }

        /// <summary>
        /// Remove all tokens of a particular kind from the stream.
        /// </summary>
        /// <param name="values">Values to remove.</param>
        public void RemoveValues(params int[] values)
        {
            RemoveAll(token => values.Contains(token.Value));
        }

        /// <summary>
        /// Consume (and skip) a number of tokens. Return how many was skipped.
        /// </summary>
        /// <param name="skiptokens"></param>
        /// <returns>Tokens skipped.</returns>
        public int Skip(params int[] skiptokens)
        {
            var count = 0;
            while (Peek(skiptokens))
            {
                Consume();
                count++;
            }

            return count;
        }

        /// <summary>
        /// Split a token list into several based on a splitting token.
        /// </summary>
        /// <param name="splitValue">Token to split on.</param>
        /// <returns>A list of tokenlists.</returns>
        public List<TokenList> Split(int splitValue)
        {
            var result = new List<TokenList>();

            var list = new TokenList();
            result.Add(list);

            foreach (var token in this)
            {
                if (token.Value == splitValue)
                    result.Add(list = new TokenList());
                else
                    list.Add(token);
            }

            return result;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var token in this)
            {
                result.Append(token.Text);
                if (token.Section.Any())
                {
                    result.Append(token.Section);
                    result.Append((token.Definition as TokenSectionDefinition)?.EndText);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Trim start and end of the list.
        /// </summary>
        /// <param name="values"></param>
        public void Trim(params int[] values)
        {
            TrimStart(values);
            TrimEnd(values);
        }

        private void TrimEnd(int[] values)
        {
            while (Count > 0 && values.Contains(this[Count-1].Value))
                RemoveAt(Count-1);
        }

        private void TrimStart(int[] values)
        {
            while (Count > 0 && values.Contains(this[0].Value))
                RemoveAt(0);
        }
    }
}
