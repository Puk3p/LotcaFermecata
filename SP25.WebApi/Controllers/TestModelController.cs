using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;

namespace SP25.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestModelController : ControllerBase
    {
        private readonly ITestService _testService;
        public TestModelController(ITestService testService)
        {
            _testService = testService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var models = _testService.GetAll();
            return Ok(models);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var model = _testService.GetById(id);
            if (model == null)
                return NotFound();
            return Ok(model);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TestModelDto model)
        {
            _testService.Create(model);
            return Ok("Succesfully created");
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] TestModelDto model)
        {
            if (id != model.Id)
                return BadRequest();

            _testService.Update(id, model);
            return Ok("Updated succesfully");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _testService.Delete(id);
            return Ok("Deleted succesfully");
        }
    }
}
