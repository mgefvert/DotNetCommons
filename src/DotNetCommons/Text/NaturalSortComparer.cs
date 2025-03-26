namespace DotNetCommons.Text;

public class NaturalSortComparer : IComparer<string>
{
    private readonly bool _caseSensitive;

    public NaturalSortComparer(bool caseSensitive)
    {
        _caseSensitive = caseSensitive;
    }
    
    public int Compare(string? a, string? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a == null) return -1;
        if (b == null) return 1;

        int ia = 0,        
            ib = 0,
            na = a.Length, 
            nb = b.Length;

        while (ia < na && ib < nb)
        {
            if (char.IsDigit(a[ia]) && char.IsDigit(b[ib]))
            {
                // Extract full number from both strings
                long va = 0, vb = 0;

                while (ia < na && char.IsDigit(a[ia]))
                    va = va * 10 + (a[ia++] - '0');

                while (ib < nb && char.IsDigit(b[ib]))
                    vb = vb * 10 + (b[ib++] - '0');

                if (va < vb) return -1;
                if (va > vb) return 1;
            }
            else
            {
                var ca = _caseSensitive ? a[ia++] : char.ToUpper(a[ia++]);
                var cb = _caseSensitive ? b[ib++] : char.ToUpper(b[ib++]);

                if (ca < cb) return -1;
                if (ca > cb) return 1;
            }
        }

        return na - nb;
    }
}
