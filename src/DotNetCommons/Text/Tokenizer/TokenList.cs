using DotNetCommons.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer;

public class TokenList<T> : List<Token<T>>
{
    public TokenList()
    {
    }

    public TokenList(IEnumerable<Token<T>> tokens)
    {
        AddRange(tokens);
    }

    /// <summary>
    /// Consume one token of a particular kind, throwing an exception if an illegal token was found.
    /// </summary>
    /// <param name="allowed">Allowed tokens.</param>
    /// <returns>A token, or null if at the end.</returns>
    public Token<T>? Consume(params T[] allowed)
    {
        var result = this.ExtractFirstOrDefault();
        if (result != null && allowed != null && allowed.Length > 0 && !allowed.Contains(result.ID))
            throw new StringTokenizerException($"Illegal token '{result.Text}' in text.");

        return result;
    }

    /// <summary>
    /// Consume allowed tokens until the end of the string, throwing an exception if an illegal token was found.
    /// </summary>
    /// <param name="allowed">Allowed tokens.</param>
    /// <returns>An enumeration of tokens.</returns>
    public IEnumerable<Token<T>> ConsumeAll(params T[] allowed)
    {
        while (Count > 0)
        {
            var result = Consume(allowed);
            if (result != null)
                yield return result;
        }
    }

    /// <summary>
    /// Consume tokens until a stop token was found. The stop token will be left on the stream.
    /// </summary>
    /// <param name="stop">Stop tokens.</param>
    /// <returns>An enumeration of tokens.</returns>
    public IEnumerable<Token<T>> ConsumeUntil(params T[] stop)
    {
        while (Count > 0 && !Peek(stop))
            yield return this.ExtractFirst();
    }

    /// <summary>
    /// Peek at the next token.
    /// </summary>
    public Token<T>? Peek()
    {
        return this.FirstOrDefault();
    }

    /// <summary>
    /// Peek at the next token and see if it's of a particular kind.
    /// </summary>
    /// <returns>True if the next token matches the allowed token list.</returns>
    public bool Peek(params T[] allowed)
    {
        var next = Peek();
        return next != null && allowed.Contains(next.ID);
    }

    /// <summary>
    /// Remove all tokens of a particular kind from the stream.
    /// </summary>
    /// <param name="values">Values to remove.</param>
    public void RemoveValues(params T[] values)
    {
        RemoveAll(token => values.Contains(token.ID));
    }

    /// <summary>
    /// Consume (and skip) a number of tokens. Return how many was skipped.
    /// </summary>
    /// <returns>Tokens skipped.</returns>
    public int Skip(params T[] skipTokens)
    {
        var count = 0;
        while (Peek(skipTokens))
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
    public List<TokenList<T>> Split(T splitValue)
    {
        var result = new List<TokenList<T>>();

        var list = new TokenList<T>();
        result.Add(list);

        foreach (var token in this)
        {
            if (token.ID != null && token.ID.Equals(splitValue))
                result.Add(list = new TokenList<T>());
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
                result.Append((token.Definition as Section<T>)?.EndText);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Trim start and end of the list.
    /// </summary>
    /// <param name="values"></param>
    public void Trim(params T[] values)
    {
        TrimStart(values);
        TrimEnd(values);
    }

    private void TrimEnd(T[] values)
    {
        while (Count > 0 && values.Contains(this[Count - 1].ID))
            RemoveAt(Count - 1);
    }

    private void TrimStart(T[] values)
    {
        while (Count > 0 && values.Contains(this[0].ID))
            RemoveAt(0);
    }
}