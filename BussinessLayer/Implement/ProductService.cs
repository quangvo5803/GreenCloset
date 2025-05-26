using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Repository.Implement;
using Utility.Media;

namespace BussinessLayer.Implement
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public ProductService(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
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
            string uploadFolder = "products";
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            // Add avatar
            if (avatar != null)
            {
                var productAvatar = new ItemImage
                {
                    ImagePath = "",
                    ProductId = product.Id,
                    PublicId = "",
                };

                //Upload to cloudinary
                await _cloudinaryService.UploadImageAsync(avatar, uploadFolder, productAvatar);

                _unitOfWork.ItemImage.Add(productAvatar);
                _unitOfWork.Save();
                product.ProductAvatarId = productAvatar.Id;
            }
            //Add gallery
            if (gallery?.Any() == true)
            {
                foreach (var file in gallery)
                {
                    var productImage = new ItemImage
                    {
                        ImagePath = "",
                        ProductId = product.Id,
                        PublicId = "",
                    };
                    //Upload to cloudinary
                    await _cloudinaryService.UploadImageAsync(file, uploadFolder, productImage);
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
            string uploadFolder = "products";
            if (avatar != null)
            {
                if (product.ProductAvatar != null)
                {
                    await _cloudinaryService.DeleteImageAsync(product.ProductAvatar.PublicId);
                }
                var newAvatar = new ItemImage
                {
                    ImagePath = "",
                    ProductId = product.Id,
                    PublicId = "",
                };
                //Upload to cloudinary
                await _cloudinaryService.UploadImageAsync(avatar, uploadFolder, newAvatar);

                _unitOfWork.ItemImage.Add(newAvatar);
                _unitOfWork.Save();
                product.ProductAvatarId = newAvatar.Id;
            }

            if (gallery != null && gallery.Count > 0)
            {
                if (product.ProductImages != null)
                {
                    foreach (var oldImage in product.ProductImages)
                    {
                        // Xóa trên Cloudinary
                        if (!string.IsNullOrEmpty(oldImage.PublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(oldImage.PublicId);
                        }

                        // Xóa trong DB
                        _unitOfWork.ItemImage.Remove(oldImage);
                    }
                }
                List<ItemImage> newGallery = new List<ItemImage>();
                foreach (var item in gallery)
                {
                    var newImage = new ItemImage
                    {
                        ImagePath = "",
                        ProductId = product.Id,
                        PublicId = "",
                    };
                    //Upload to cloudinary
                    await _cloudinaryService.UploadImageAsync(item, uploadFolder, newImage);
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

        public async Task<bool> DeleteProduct(Product product)
        {
            if (product.ProductAvatar != null || product.ProductImages != null)
            {
                //Remove avatar
                if (product.ProductAvatar != null)
                {
                    await _cloudinaryService.DeleteImageAsync(product.ProductAvatar.PublicId);
                    _unitOfWork.ItemImage.Remove(product.ProductAvatar);
                }
                //Remove gallery
                if (product.ProductImages != null)
                {
                    foreach (var image in product.ProductImages)
                    {
                        if (image.Id != product.ProductAvatarId)
                        {
                            await _cloudinaryService.DeleteImageAsync(image.PublicId);
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
    }
}
