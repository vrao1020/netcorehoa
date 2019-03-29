namespace HoaCommon.Services
{
    public interface IPaginationGenerator
    {
        void GenerateHeaders(int totalCount, int pageSize, int pageNumber, string sorts, string filters);
    }
}
