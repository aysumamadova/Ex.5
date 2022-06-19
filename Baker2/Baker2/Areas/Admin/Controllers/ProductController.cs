using Baker2.DAL;
using Baker2.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Baker2.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _env = hostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(product);
                Product p = new Product
                {
                    Name = product.Name,
                    Desc = product.Desc,
                    Img = uniqueFileName
                };

                _context.Add(p);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        private string ProcessUploadedFile(Product product)
        {
            string uniqueFileName = null;

            if (product.Photo != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "assets","img");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + product.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    product.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Product product = _context.Products.Find(id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var p = await _context.Products.FindAsync(id);
            var product = new Product()
            {
                Id = p.Id,
                Name = p.Name,
                Desc =p.Desc,
                Img = p.Img,
            };

            if (p == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (ModelState.IsValid)
            {
                var p = await _context.Products.FindAsync(product.Id);
                p.Name = product.Name;
                p.Desc = product.Desc;
                if (product.Photo != null)
                {
                    if (product.Img != null)
                    {
                        string filePath = Path.Combine(_env.WebRootPath, "assets", "img", product.Img);
                        System.IO.File.Delete(filePath);
                    }

                    p.Img = ProcessUploadedFile(product);
                }
                _context.Update(p);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

    }
}
