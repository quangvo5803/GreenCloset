using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Repository.Implement;

namespace BussinessLayer.Implement
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public Product? GetProductById(int id, string? includeProperties = null)
        {
            return _unitOfWork.Product.Get(p => p.Id == id, includeProperties);
        }

        public IEnumerable<Product> GetAllProducts(string? includeProperties = null)
        {
            return _unitOfWork.Product.GetAll(includeProperties);
        }

        public IEnumerable<Product> GetLessorProducts(
            string? email = null,
            string? includeProperties = null
        )
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                return _unitOfWork.Product.GetAll();
            }
            return _unitOfWork.Product.GetRange(p => p.UserId == user.Id, includeProperties);
        }

        public IEnumerable<Product> GetFeatureProduct(string? includeProperties = null)
        {
            return _unitOfWork
                .Product.GetAll(includeProperties)
                .Where(p => p.RentalCount > 0)
                .OrderByDescending(p => p.RentalCount)
                .Take(8);
        }

        public IEnumerable<Product> GetSimilarProduct(
            Product product,
            string? includeProperties = null
        )
        {
            return _unitOfWork
                .Product.GetAll(includeProperties)
                .Where(p => p.Id != product.Id)
                .Where(p =>
                    product.Categories != null
                    && p.Categories != null
                    && p.Categories.Any(c => product.Categories.Contains(c))
                )
                .OrderBy(p => new Random().Next())
                .Take(4);
        }

        public async Task AddProduct(
            Guid userId,
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        )
        {
            product.UserId = userId;
            // Add product
            _unitOfWork.Product.Add(product);
            if (selectedCategories != null)
            {
                List<Category> categories = new List<Category>();
                foreach (var categoryId in selectedCategories)
                {
                    var category = _unitOfWork.Category.Get(c => c.Id == categoryId);
                    if (category != null)
                    {
                        categories.Add(category);
                    }
                }
                product.Categories = categories;
            }
            _unitOfWork.Save();
            // Create folder for images
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image/products");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            // Add avatar
            if (avatar != null)
            {
                //Add to folder
                string avatarFileName = await SaveImageAsync(avatar, uploadFolder);
                //Save to database
                var productAvatar = new ItemImage
                {
                    ImagePath = avatarFileName,
                    ProductId = product.Id,
                };
                try
                {
                    _unitOfWork.ItemImage.Add(productAvatar);
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    // Log hoặc xem ex.Message, ex.StackTrace để biết lỗi gì
                    Console.WriteLine(ex.Message);
                    throw; // hoặc xử lý khác
                }
                product.ProductAvatarId = productAvatar.Id;
            }
            //Add gallery
            if (gallery?.Any() == true)
            {
                foreach (var file in gallery)
                {
                    //Add to folder
                    string fileName = await SaveImageAsync(file, uploadFolder);
                    //Save to database
                    var productImage = new ItemImage
                    {
                        ImagePath = fileName,
                        ProductId = product.Id,
                    };
                    _unitOfWork.ItemImage.Add(productImage);
                }
            }
            _unitOfWork.Save();
        }

        public async Task UpdateProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        )
        {
            product.Categories = null;
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();
            if (selectedCategories != null)
            {
                List<Category> categories = new List<Category>();
                foreach (var categoryId in selectedCategories)
                {
                    var category = _unitOfWork.Category.Get(c => c.Id == categoryId);
                    if (category != null)
                    {
                        categories.Add(category);
                    }
                }
                product.Categories = categories;
            }
            _unitOfWork.Save();
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image/products");
            if (avatar != null)
            {
                if (product.ProductAvatar != null)
                {
                    var oldAvatarPath = Path.Combine(uploadFolder, product.ProductAvatar.ImagePath);
                    if (File.Exists(oldAvatarPath))
                    {
                        File.Delete(oldAvatarPath);
                    }
                    _unitOfWork.ItemImage.Remove(product.ProductAvatar);
                }

                var newAvatarFileName = await SaveImageAsync(avatar, uploadFolder);

                var newAvatar = new ItemImage
                {
                    ImagePath = newAvatarFileName,
                    ProductId = product.Id,
                };
                _unitOfWork.ItemImage.Add(newAvatar);
                _unitOfWork.Save();
                product.ProductAvatarId = newAvatar.Id;
            }

            if (gallery != null && gallery.Count > 0)
            {
                List<ItemImage> newGallery = new List<ItemImage>();
                foreach (var item in gallery)
                {
                    var newImageFileName = await SaveImageAsync(item, uploadFolder);
                    var newImage = new ItemImage
                    {
                        ImagePath = newImageFileName,
                        ProductId = product.Id,
                    };
                    newGallery.Add(newImage);
                }

                _unitOfWork.ItemImage.AddRange(newGallery);
                if (product.ProductImages != null)
                {
                    foreach (var image in product.ProductImages)
                    {
                        newGallery.Add(image);
                    }
                }
                product.ProductImages = newGallery;
            }
            _unitOfWork.Save();
        }

        public bool DeleteProduct(Product product)
        {
            if (product.ProductAvatar != null || product.ProductImages != null)
            {
                string uploadFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "image/products"
                );
                //Remove avatar
                if (product.ProductAvatar != null)
                {
                    var oldAvatarPath = Path.Combine(uploadFolder, product.ProductAvatar.ImagePath);
                    if (File.Exists(oldAvatarPath))
                    {
                        File.Delete(oldAvatarPath);
                    }
                    _unitOfWork.ItemImage.Remove(product.ProductAvatar);
                }
                //Remove gallery
                if (product.ProductImages != null)
                {
                    foreach (var image in product.ProductImages)
                    {
                        if (image.Id != product.ProductAvatarId)
                        {
                            var oldImagePath = Path.Combine(uploadFolder, image.ImagePath);
                            if (File.Exists(oldImagePath))
                            {
                                File.Delete(oldImagePath);
                            }
                            _unitOfWork.ItemImage.Remove(image);
                        }
                    }
                }
            }
            product.Categories = null;
            _unitOfWork.Save();
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            return true;
        }

        public IEnumerable<Product> GetProductsByFilter(ProductFilter? filter = null)
        {
            var productList = GetAllProducts(includeProperties: "Categories,ProductAvatar");
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    productList = productList.Where(p =>
                        p.Name.ToLower().Contains(filter.Search.ToLower())
                        || (
                            p.Categories != null
                            && p.Categories.Any(c =>
                                c.CategoryName.ToLower().Contains(filter.Search.ToLower())
                            )
                        )
                        || (
                            p.Description != null
                            && p.Description.ToLower().Contains(filter.Search.ToLower())
                        )
                    );
                }

                if (filter.CategoryIds != null && filter.CategoryIds.Any())
                {
                    productList = productList.Where(p =>
                        p.Categories != null
                        && p.Categories.Any(c => filter.CategoryIds.Contains(c.Id))
                    );
                }

                if (filter.Colors != null && filter.Colors.Any())
                {
                    productList = productList.Where(p =>
                        p.Color != null && filter.Colors.Contains(p.Color.Value)
                    );
                }

                if ((filter.ClotherSizes?.Any() ?? false) || (filter.ShoeSizes?.Any() ?? false))
                {
                    productList = productList.Where(p =>
                        (filter.ClotherSizes?.Any() ?? false)
                            && (p.SizeClother?.Any(s => filter.ClotherSizes.Contains(s)) ?? false)
                        || (filter.ShoeSizes?.Any() ?? false)
                            && (p.SizeShoe?.Any(s => filter.ShoeSizes.Contains(s)) ?? false)
                    );
                }

                if (filter.PriceFrom.HasValue)
                {
                    productList = productList.Where(p => p.Price >= filter.PriceFrom.Value);
                }

                if (filter.PriceFrom.HasValue)
                {
                    productList = productList.Where(p => p.Price <= filter.PriceFrom.Value);
                }
            }

            return productList;
        }

        private async Task<string> SaveImageAsync(IFormFile file, string uploadFolder)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}
