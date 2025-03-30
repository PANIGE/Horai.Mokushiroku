using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Horai.Mokushiroku.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

public static class EncounterPdfGenerator
{
    private static PdfFont boldFont { get; set; }

    public static string Generate(List<EncounterProfile> data)
    {
        string path = Path.Combine(Path.GetTempPath(), $"Encounters_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        var writer = new PdfWriter(path);

        using var pdf = new PdfDocument(writer);
        var doc = new Document(pdf);

        PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        PdfFont bodyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
        boldFont = headerFont;

        var categories = data.Select(p => p.Category).Distinct().ToList();
        var categoryAnchors = new HashSet<string>();

        // Page de garde
        doc.Add(new Paragraph("🌌 Liste des Encounters 🌌")
            .SetFont(headerFont)
            .SetFontSize(22)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(30));

        doc.Add(new Paragraph("📑 Sommaire par catégorie :")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetUnderline()
            .SetMarginBottom(10));

        foreach (var cat in categories)
        {
            var anchor = $"cat_{cat.ToLower().Replace(" ", "_")}";
            var link = new Link($"• {cat}", PdfAction.CreateGoTo(anchor));
            doc.Add(new Paragraph(link)
                .SetFontSize(12)
                .SetFontColor(ColorConstants.DARK_GRAY));
        }

        doc.Add(new Paragraph("📘 Sommaire par entrée :")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetUnderline()
            .SetMarginTop(20)
            .SetMarginBottom(10));

        foreach (var profilegroup in data.GroupBy(s => s.Category))
        {
            doc.Add(new Paragraph(profilegroup.Key)
                .SetFont(boldFont)
                .SetFontSize(13)
                .SetUnderline()
                .SetFontColor(ColorConstants.BLACK)
                .SetUnderline()
                .SetMarginBottom(10));
            foreach (EncounterProfile profile in profilegroup)
            {
                var anchor = $"entry_{profile.Name.ToLower().Replace(" ", "_")}";
                var link = new Link($"• {profile.Name}", PdfAction.CreateGoTo(anchor));
                doc.Add(new Paragraph(link)
                .SetFontSize(11)
                .SetMarginLeft(10)
                .SetFontColor(ColorConstants.GRAY));
            }

        }

        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

        for (int i = 0; i < data.Count; i++)
        {
            var profile = data[i];
            string categoryAnchor = $"cat_{profile.Category.ToLower().Replace(" ", "_".ToString())}";
            string entryAnchor = $"entry_{profile.Name.ToLower().Replace(" ", "_")}";

            if (!categoryAnchors.Contains(profile.Category))
            {
                categoryAnchors.Add(profile.Category);
                doc.Add(new Paragraph($"📂 {profile.Category}")
                    .SetDestination(categoryAnchor)
                    .SetFontSize(16)
                    .SetFont(boldFont)
                    .SetFontColor(ColorConstants.BLUE));
            }

            doc.Add(new Paragraph($"🧿 {profile.Name}")
                .SetDestination(entryAnchor)
                .SetFont(headerFont)
                .SetFontSize(16)
                .SetFontColor(ColorConstants.BLUE));

            doc.Add(new Paragraph($"📂 Catégorie : {profile.Category}")
                .SetFont(bodyFont)
                .SetFontSize(12));

            doc.Add(new Paragraph($"📝 Description : {profile.Description}")
                .SetFont(italicFont)
                .SetFontSize(11));

            doc.Add(new Paragraph($"👥 Groupe : {profile.Group.Min} - {profile.Group.Max}")
                .SetFont(bodyFont));

            doc.Add(new Paragraph($"🎁 Drop Rate : {profile.DropRate}%")
                .SetFont(bodyFont));

            if (profile.Status.Any())
                AddListSection(doc, "📈 Humeur", profile.Status);

            AddDropRateSection(doc, "🎭 Genre", profile.Genre);
            AddDropRateSection(doc, "⚡ Puissance", profile.PowerLevel);
            AddDropRateSection(doc, "🔷 Type 1", profile.Type1);
            AddDropRateSection(doc, "🔶 Type 2", profile.Type2);
            AddDropRateSection(doc, "☠️ Corruption", profile.Corruption);

            if (i < data.Count - 1)
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }

        doc.Close();
        return path;
    }

    private static void AddDropRateSection(Document doc, string title, List<HasDropRate> items)
    {
        if (!items.Any()) return;

        doc.Add(new Paragraph(title).SetFont(boldFont).SetFontSize(12));

        var list = new List().SetListSymbol("• ").SetFontSize(11);
        foreach (var item in items)
        {
            list.Add(new ListItem($"{item.Name} ({item.DropRate}%)"));
        }

        doc.Add(list);
    }

    private static void AddListSection(Document doc, string title, List<string> items)
    {
        if (!items.Any()) return;

        doc.Add(new Paragraph(title).SetFont(boldFont).SetFontSize(12));

        var list = new List().SetListSymbol("• ").SetFontSize(11);
        foreach (var item in items)
        {
            list.Add(new ListItem(item));
        }

        doc.Add(list);
    }
}
