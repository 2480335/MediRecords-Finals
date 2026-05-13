namespace MediRecords.Dto.BillingDtos.Response;
public class PagedResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount  { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}