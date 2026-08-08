using Lms.SharedKernel.Pagination;

namespace Lms.UnitTests.Pagination;

public class PageRequestTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(7, 7)]
    public void Page_is_at_least_one(int? requested, int expected) =>
        PageRequest.Of(requested, null).Page.ShouldBe(expected);

    [Theory]
    [InlineData(null, PageRequest.DefaultPageSize)]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    [InlineData(20, 20)]
    [InlineData(50, 50)]
    [InlineData(51, PageRequest.MaxPageSize)]
    [InlineData(5000, PageRequest.MaxPageSize)]
    public void PageSize_is_clamped_not_rejected(int? requested, int expected) =>
        PageRequest.Of(null, requested).PageSize.ShouldBe(expected);

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 15, 30)]
    public void Skip_is_derived_from_page_and_size(int page, int size, int expectedSkip) =>
        PageRequest.Of(page, size).Skip.ShouldBe(expectedSkip);

    [Fact]
    public void First_is_page_one_at_the_default_size()
    {
        PageRequest.First.Page.ShouldBe(1);
        PageRequest.First.PageSize.ShouldBe(PageRequest.DefaultPageSize);
    }
}

public class QueryResultTests
{
    [Fact]
    public void Empty_has_no_data_and_a_zero_count()
    {
        QueryResult<string>.Empty.Data.ShouldBeEmpty();
        QueryResult<string>.Empty.TotalCount.ShouldBe(0);
    }

    [Fact]
    public void Map_projects_the_data_and_preserves_the_total()
    {
        var source = new QueryResult<int>([1, 2, 3], TotalCount: 97);

        var mapped = source.Map(i => i.ToString());

        mapped.Data.ShouldBe(["1", "2", "3"]);
        mapped.TotalCount.ShouldBe(97, "the total is the row count, not the page size");
    }
}

public class PagedResultTests
{
    [Fact]
    public void From_carries_the_page_across_and_renames_Data_to_Items()
    {
        var query = new QueryResult<int>([1, 2, 3], TotalCount: 137);

        var paged = PagedResult<int>.From(query, PageRequest.Of(2, 20));

        paged.Items.ShouldBe([1, 2, 3]);
        paged.Page.ShouldBe(2);
        paged.PageSize.ShouldBe(20);
        paged.TotalCount.ShouldBe(137);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(19, 20, 1)]
    [InlineData(20, 20, 1)]   // exact multiple must not produce a trailing empty page
    [InlineData(21, 20, 2)]
    [InlineData(137, 20, 7)]
    [InlineData(40, 20, 2)]
    public void TotalPages_rounds_up_without_an_off_by_one(int totalCount, int pageSize, int expectedPages)
    {
        var query = new QueryResult<int>([], totalCount);

        PagedResult<int>.From(query, PageRequest.Of(1, pageSize)).TotalPages.ShouldBe(expectedPages);
    }

    [Fact]
    public void Empty_reports_zero_pages()
    {
        var paged = PagedResult<string>.Empty(PageRequest.First);

        paged.Items.ShouldBeEmpty();
        paged.TotalCount.ShouldBe(0);
        paged.TotalPages.ShouldBe(0);
    }
}
