using System.Globalization;

namespace Lkvitai.Warehouse.Infrastructure.Export;

public static class CsvValidation
{
    // ожидаем заголовок ровно в таком порядке
    public static readonly string[] Header = new[]{
        "ExportAt","SliceType","SliceKey","ItemCode","BaseUoM","QtyBase","DisplayQty","AdjValue","BatchCode","LocationCode","Quality","ExpDate"
    };

    public sealed record RowError(int LineNo, string Message);

    public static (bool ok, List<RowError> errors) Validate(string[] lines)
    {
        var errors = new List<RowError>();
        if (lines.Length == 0) { errors.Add(new(1,"empty file")); return (false, errors); }

        var header = SplitCsv(lines[0]);
        if (!Header.SequenceEqual(header))
            errors.Add(new(1,$"header mismatch; got: {string.Join(",",header)}"));

        for (int i=1;i<lines.Length;i++)
        {
            var cols = SplitCsv(lines[i]);
            if (cols.Length != Header.Length) { errors.Add(new(i+1,"wrong column count")); continue; }

            bool Req(int idx, string name){ if (string.IsNullOrWhiteSpace(cols[idx])) { errors.Add(new(i+1,$"{name} required")); return false; } return true; }

            Req(0,"ExportAt"); Req(1,"SliceType"); Req(2,"SliceKey"); Req(3,"ItemCode"); Req(4,"BaseUoM"); Req(5,"QtyBase");
            // числа
            if (!decimal.TryParse(cols[5], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                errors.Add(new(i+1,"QtyBase not decimal"));
            if (!string.IsNullOrWhiteSpace(cols[7]) && !decimal.TryParse(cols[7], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                errors.Add(new(i+1,"AdjValue not decimal"));
            // дата
            if (!string.IsNullOrWhiteSpace(cols[11]) && !DateTime.TryParse(cols[11], CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                errors.Add(new(i+1,"ExpDate invalid"));
        }
        return (errors.Count==0, errors);
    }

    // минимальный CSV split: "a","b",c
    private static string[] SplitCsv(string line)
    {
        var res = new List<string>();
        bool q=false; var cur="";
        for (int i=0;i<line.Length;i++)
        {
            var ch=line[i];
            if (ch=='"')
            {
                if (q && i+1<line.Length && line[i+1]=='"') { cur+='"'; i++; }
                else q=!q;
            }
            else if (ch==',' && !q) { res.Add(cur); cur=""; }
            else cur+=ch;
        }
        res.Add(cur);
        return res.ToArray();
    }

    public static string BuildErrorsCsv(IEnumerable<RowError> errs)
    {
        var lines = new List<string>() { "LineNo,Message" };
        lines.AddRange(errs.Select(e => $"{e.LineNo},{Escape(e.Message)}"));
        return string.Join(Environment.NewLine, lines);
    }
    private static string Escape(string s) => s.Contains(',') ? $"\"{s.Replace("\"","\"\"")}\"" : s;
}
