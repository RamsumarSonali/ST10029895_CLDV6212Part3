using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;
using ABCRetailers.Constants;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IAzureStorageService storageService, ILogger<ProductController> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _storageService.GetAllEntitiesAsync<Product>();

                // DEBUGGING: Log prices to see what's happening
                foreach (var p in products)
                {
                    _logger.LogInformation($"Product: {p.ProductName}, PriceCents: {p.PriceCents}, Price: {p.Price}");
                }

                return View(products.Where(p => p.IsActive).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["Error"] = "Error loading products. Please try again.";
                return View(new List<Product>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            // DEBUGGING: Log what was received
            _logger.LogInformation($"Create POST - Price from form: {product.Price}, PriceCents: {product.PriceCents}");

            
            
                try
                {
                    if (product.Price <= 0)
                    {
                        ModelState.AddModelError("Price", "Price must be greater than $0.00");
                        return View(product);
                    }

                    // Set properties
                    product.RowKey = Guid.NewGuid().ToString();
                    product.PartitionKey = StorageConstants.ProductPartitionKey;
                    product.DateAdded = DateTime.UtcNow;
                    product.IsActive = true;

                    // CRITICAL: Ensure PriceCents is set correctly
                    // The setter should have already done this, but let's be explicit
                    if (product.PriceCents == 0 && product.Price > 0)
                    {
                        product.PriceCents = (int)Math.Round(product.Price * 100);
                    }

                    _logger.LogInformation($"Saving product with Price: {product.Price}, PriceCents: {product.PriceCents}");

                    // Upload image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, StorageConstants.ProductImagesContainer);
                        product.ImageUrl = imageUrl;
                    }

                    await _storageService.AddEntityAsync(product);

                    TempData["Success"] = $"Product '{product.ProductName}' created successfully with price {product.Price:C}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                }
            
            

            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _storageService.GetEntityAsync<Product>(StorageConstants.ProductPartitionKey, id);
            if (product == null)
            {
                return NotFound();
            }

            // DEBUGGING: Log what we're editing
            _logger.LogInformation($"Edit GET - Product: {product.ProductName}, PriceCents: {product.PriceCents}, Price: {product.Price}");

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            // DEBUGGING: Log what was received
            _logger.LogInformation($"Edit POST - Price from form: {product.Price}, PriceCents: {product.PriceCents}");

            if (ModelState.IsValid)
            {
                try
                {
                    var originalProduct = await _storageService.GetEntityAsync<Product>(
                        StorageConstants.ProductPartitionKey, product.RowKey);

                    if (originalProduct == null)
                    {
                        return NotFound();
                    }

                    // Update properties
                    originalProduct.ProductName = product.ProductName;
                    originalProduct.Description = product.Description;
                    originalProduct.StockAvailable = product.StockAvailable;
                    originalProduct.Category = product.Category;

                    // CRITICAL: Update price explicitly
                    originalProduct.Price = product.Price; // This will set PriceCents via the setter

                    // Double-check PriceCents is set
                    if (originalProduct.PriceCents == 0 && product.Price > 0)
                    {
                        originalProduct.PriceCents = (int)Math.Round(product.Price * 100);
                    }

                    _logger.LogInformation($"Updating product - Price: {originalProduct.Price}, PriceCents: {originalProduct.PriceCents}");

                    // Upload new image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, StorageConstants.ProductImagesContainer);
                        originalProduct.ImageUrl = imageUrl;
                    }

                    await _storageService.UpdateEntityAsync(originalProduct);

                    TempData["Success"] = $"Product updated successfully! Price: {originalProduct.Price:C}";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product");
                    ModelState.AddModelError("", "Error updating product. Please try again.");
                }
            }
            else
            {
                // DEBUGGING: Log validation errors
                foreach (var error in ModelState)
                {
                    _logger.LogWarning($"ModelState Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var product = await _storageService.GetEntityAsync<Product>(StorageConstants.ProductPartitionKey, id);
                if (product != null)
                {
                    product.IsActive = false;
                    await _storageService.UpdateEntityAsync(product);
                    TempData["Success"] = "Product deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}