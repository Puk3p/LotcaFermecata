using AutoMapper;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using SP25.Domain.Models;
using SP25.Domain.Repository;

namespace SP25.Business.Services.Implementations
{
    public class TestService : ITestService
    {
        private readonly IRepository<TestModel> _repository;
        private readonly IMapper _mapper;

        public TestService(IRepository<TestModel> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public IEnumerable<TestModelDto> GetAll()
        {
            var models = _repository.GetAll();

            return _mapper.Map<IEnumerable<TestModelDto>>(models);
        }

        public TestModelDto GetById(Guid id)
        {
            var model = _repository.GetById(id);
            if (model == null) return null;

            return _mapper.Map<TestModelDto>(model);
        }

        public void Create(TestModelDto model)
        {
            var entity = new TestModel
            {
                Name = model.Name,
                Description = model.Description
            };
            _repository.Add(entity);
            _repository.SaveChanges();
        }

        public void Update(Guid id, TestModelDto model)
        {
            var entity = _repository.GetById(id);
            if (entity == null) throw new KeyNotFoundException("Entity not found");

            entity.Name = model.Name;
            entity.Description = model.Description;
            _repository.Update(entity);
            _repository.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null) throw new KeyNotFoundException("Entity not found");

            _repository.Remove(entity);
            _repository.SaveChanges();
        }
    }
}
