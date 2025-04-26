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

        public IEnumerable<Product> GetFeatureProduct(string? includeProperties = null)
        {
            return _unitOfWork
                .Product.GetAll(includeProperties)
                .Where(p => p.RentalCount > 0)
                .OrderByDescending(p => p.RentalCount)
                .Take(8);
        }

        public IEnumerable<Product> GetProductsByCategoryId(
            int categoryId,
            string? includeProperties = null
        )
        {
            return _unitOfWork.Product.GetRange(
                p => p.Categories != null && p.Categories.Any(c => c.Id == categoryId),
                includeProperties
            );
        }

        public async Task AddProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        )
        {
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
                _unitOfWork.ItemImage.Add(productAvatar);
                _unitOfWork.Save();
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

        public IEnumerable<Product> GetProductsByFilter(
            string? search,
            List<int>? categoryIds,
            List<ProductColor>? colors,
            List<SizeClother>? clotherSizes,
            List<int>? shoeSizes,
            double? priceFrom,
            double? priceTo
        )
        {
            var productList = GetAllProducts(includeProperties: "Categories,ProductAvatar");
            if (!string.IsNullOrEmpty(search))
            {
                productList = productList.Where(p =>
                    p.Name.ToLower().Contains(search.ToLower())
                    || (
                        p.Categories != null
                        && p.Categories.Any(c =>
                            c.CategoryName.ToLower().Contains(search.ToLower())
                        )
                    )
                    || (p.Description != null && p.Description.ToLower().Contains(search.ToLower()))
                );
            }

            if (categoryIds != null && categoryIds.Any())
            {
                productList = productList.Where(p =>
                    p.Categories != null && p.Categories.Any(c => categoryIds.Contains(c.Id))
                );
            }

            if (colors != null && colors.Any())
            {
                productList = productList.Where(p =>
                    p.Color != null && colors.Contains(p.Color.Value)
                );
            }

            if (
                (clotherSizes != null && clotherSizes.Any())
                || (shoeSizes != null && shoeSizes.Any())
            )
            {
                productList = productList.Where(p =>
                    (
                        clotherSizes != null
                        && clotherSizes.Any()
                        && p.SizeClother != null
                        && p.SizeClother.Any(s => clotherSizes.Contains(s))
                    )
                    || (
                        shoeSizes != null
                        && shoeSizes.Any()
                        && p.SizeShoe != null
                        && p.SizeShoe.Any(s => shoeSizes.Contains(s))
                    )
                );
            }

            if (priceFrom.HasValue)
            {
                productList = productList.Where(p => p.Price >= priceFrom.Value);
            }

            if (priceTo.HasValue)
            {
                productList = productList.Where(p => p.Price <= priceTo.Value);
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
