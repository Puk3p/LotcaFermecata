using SP25.Business.ModelDTOs;

namespace SP25.Business.Services.Contracts
{
    public interface ITestService
    {
        IEnumerable<TestModelDto> GetAll();

        TestModelDto GetById(Guid id);

        void Create(TestModelDto model);

        void Update(Guid id, TestModelDto model);

        void Delete(Guid id);
    }
}
