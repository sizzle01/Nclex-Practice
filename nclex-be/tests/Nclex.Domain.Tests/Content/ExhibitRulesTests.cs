using System.Text.Json;
using Nclex.Domain.Content;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Content;

public sealed class ExhibitRulesTests
{
    private static readonly Guid ImageId = Guid.Parse("f11a0000-0000-4000-8000-000000000001");

    [Fact]
    public void Every_kind_of_well_formed_exhibit_passes_and_its_media_are_listed()
    {
        var check = ExhibitRules.Check(Json($$"""
        [
          { "id": "v", "kind": "flowsheet", "title": "Vitals", "table": { "columns": ["08:00", "12:00"], "rows": [ { "label": "HR", "cells": ["80", "96"] } ] } },
          { "id": "l", "kind": "table", "title": "Labs", "table": { "columns": ["Result"], "rows": [ { "label": "K", "cells": ["3.4"] } ] } },
          { "id": "n", "kind": "text", "title": "Note", "body": "Client is shaky." },
          { "id": "i", "kind": "image", "title": "Sacrum", "mediaId": "{{ImageId}}" }
        ]
        """));

        check.Issues.ShouldBeEmpty();
        check.Media.ShouldHaveSingleItem().ShouldBe(new MediaReference("exhibits[3].mediaId", ImageId, "image"));
    }

    [Fact]
    public void No_exhibits_is_fine()
    {
        ExhibitRules.Check(Json("[]")).Issues.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("""{ "exhibits": [] }""", "exhibits", ValidationIssueCodes.InvalidShape)]
    [InlineData("""[ { "kind": "text", "title": "Note", "body": "x" } ]""", "exhibits[0].id", ValidationIssueCodes.Required)]
    [InlineData("""[ { "id": "a", "kind": "text", "title": "Note", "body": "x" }, { "id": "a", "kind": "text", "title": "Note", "body": "y" } ]""", "exhibits[1].id", ValidationIssueCodes.DuplicateId)]
    [InlineData("""[ { "id": "a", "kind": "text", "body": "x" } ]""", "exhibits[0].title", ValidationIssueCodes.Required)]
    [InlineData("""[ { "id": "a", "kind": "chart", "title": "Chart" } ]""", "exhibits[0].kind", ValidationIssueCodes.InvalidValue)]
    [InlineData("""[ { "id": "a", "kind": "text", "title": "Note" } ]""", "exhibits[0].body", ValidationIssueCodes.Required)]
    [InlineData("""[ { "id": "a", "kind": "flowsheet", "title": "Vitals" } ]""", "exhibits[0].table", ValidationIssueCodes.Required)]
    [InlineData("""[ { "id": "a", "kind": "table", "title": "Labs", "table": { "columns": ["A", "B"], "rows": [ { "label": "K", "cells": ["1"] } ] } } ]""", "exhibits[0].table.rows[0].cells", ValidationIssueCodes.Mismatch)]
    [InlineData("""[ { "id": "a", "kind": "table", "title": "Labs", "table": { "columns": ["A"], "rows": [ { "cells": ["1"] } ] } } ]""", "exhibits[0].table.rows[0].label", ValidationIssueCodes.Required)]
    [InlineData("""[ { "id": "a", "kind": "audio", "title": "Lungs", "mediaId": "not-a-guid" } ]""", "exhibits[0].mediaId", ValidationIssueCodes.Required)]
    public void A_malformed_exhibit_is_reported_where_it_is(string exhibits, string path, string code)
    {
        ExhibitRules.Check(Json(exhibits)).Issues.ShouldContain(i => i.Path == path && i.Code == code);
    }

    [Fact]
    public void Graphic_options_anywhere_in_the_content_are_found()
    {
        var media = ExhibitRules.MediaInContent(Json($$"""
        { "groups": [ { "id": "g1", "options": [ { "id": "a", "text": "A", "mediaId": "{{ImageId}}" }, { "id": "b", "text": "B", "mediaId": "nope" } ] } ] }
        """));

        media.ShouldBe(
        [
            new MediaReference("content.groups[0].options[0].mediaId", ImageId, null),
            new MediaReference("content.groups[0].options[1].mediaId", Guid.Empty, null),
        ]);
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;
}
