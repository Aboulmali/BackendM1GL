namespace BackendM1GL.DTOs.Common
{
    public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
}
