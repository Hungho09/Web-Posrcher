using Microsoft.AspNetCore.Mvc;
using TH3.Repositories;

namespace TH3.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryMenuViewComponent(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }
    }
}
