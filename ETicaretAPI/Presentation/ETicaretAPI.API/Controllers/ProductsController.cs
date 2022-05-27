﻿
using ETicaretAPI.Application.Repositories;
using ETicaretAPI.Application.RequestParameters;
using ETicaretAPI.Application.ViewModels.Products;
using ETicaretAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ETicaretAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        readonly private IProductWriteRepository _productWriteRepository;
        readonly private IProductReadRepository _productReadRepository;
        private readonly IWebHostEnvironment  _webHostEnviroment;

        public ProductsController(
            IProductWriteRepository productWriteRepository,
            IProductReadRepository productReadRepository,
            IWebHostEnvironment webHostEnviroment)


        {
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            this._webHostEnviroment = webHostEnviroment;
        }

        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Pagination pagination)

        {
          
            var totalCount = _productReadRepository.GetAll(false).Count();

            var products=_productReadRepository.GetAll(false).Skip(pagination.Page * pagination.Size).Take(pagination.Size).Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.CreatedDate,
                p.UpdatedDate



            }).ToList();

            return Ok(new
            {
                totalCount,
                products
                
            });

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {

            return Ok(await _productReadRepository.GetByIdAsync(id,false));


        }
      
        [HttpPost]
        public async Task<IActionResult> Post(VM_Create_Product model)
        {

          await   _productWriteRepository.AddAsync(new()
            {
                Name=model.Name,
                Price=model.Price,  
                Stock=model.Stock,
            });
            await _productWriteRepository.SaveAsync();
            return StatusCode((int)HttpStatusCode.Created);
        }
        
        [HttpPut]
        public async Task<IActionResult> Put(VM_Update_Product model)
        {
            Product product=  await _productReadRepository.GetByIdAsync(model.Id);
            product.Stock = model.Stock;
            product.Price = model.Price;    
            product.Name  = model.Name;
            await _productWriteRepository.SaveAsync();

            return Ok();
        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(string id)
        {
            await _productWriteRepository.RemoveAsync(id);
            await _productWriteRepository.SaveAsync();
            return Ok();
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload()
        {
            //wwwwroot/resource/product-images
            string uploadPath = Path.Combine(_webHostEnviroment.WebRootPath,"resource/product-images");

            if(!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            Random r = new();

            foreach (IFormFile file in Request.Form.Files)
            {
                string fullPath = Path.Combine(uploadPath,$"{r.Next()}{Path.GetExtension(file.Name)}");
                  

                using FileStream fileStream = new(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: false);
                await file.CopyToAsync(fileStream);
                await fileStream.FlushAsync();


            }
            return Ok();
        }





    }

}

