﻿using Blog.Application.AppServices;
using Blog.Application.Interfaces;
using Blog.Application.ViewModels;
using Blog.Domain.NotMapped;
using Blog.Infra.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ApiJwtBlogDotnetCore6.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostsController : Controller
    {
        AutenticacaoContext applicationDbContext;
        IWebHostEnvironment _hostingEnvironment;
        IHttpContextAccessor _httpContextAccessor;
        IPostAppService _postAppService;

        public PostsController(IWebHostEnvironment hostEnvironment, IHttpContextAccessor iHttpContextAccessor, IPostAppService postAppService)
        {
            applicationDbContext = new AutenticacaoContext(new DbContextOptions<AutenticacaoContext>());
            this._hostingEnvironment = hostEnvironment;
            this._httpContextAccessor = iHttpContextAccessor;
            this._postAppService = postAppService;
        }

        
        [HttpGet]
        public async Task<IActionResult> Index(int? offset = 0, int? limit = 10, string? buscar = null)
        {

            //var query = from p in applicationDbContext.Posts select new  {p.Id, p.Titulo, p.Descricao, p.DataCadastroFormatada};
            //var query = from p in applicationDbContext.Posts where p.Id == 8 select p;
            //var query = from p in applicationDbContext.Posts select p;

            /*
            var query = from p in applicationDbContext.Posts join 
                        c in applicationDbContext.Comentarios on p.Id equals c.postid
                        where p.Id == 8 select new { p.Id, p.Titulo, p.Descricao, p.DataCadastroFormatada, c.Id,c.Titulo,c.Descricao };
            */
            //var query = applicationDbContext.Posts.FromSqlRaw("select * from posts").ToList();

            //return Content(JsonConvert.SerializeObject(query));

            try
            {
                //List<PostsViewModel> posts = await applicationDbContext.Posts.Select(x => new PostsViewModel { Id = x.Id, Titulo = x.Titulo, Descricao = x.Descricao, DataCadastro = x.DataCadastro }).ToListAsync();
                var listaPosts = await applicationDbContext.Posts
                    .Where(x => x.Titulo.Contains(buscar != null ? buscar : x.Titulo)
                    || x.Descricao.Contains(buscar != null ? buscar : x.Descricao))
                    .Skip((int)offset * (int)limit)
                        .Take((int)limit)
                        .ToListAsync();

                return Content(JsonConvert.SerializeObject(listaPosts));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("/Posts/{id}")]
        public ActionResult Details([FromRoute] int id)
        {
            try
            {
                var post = _postAppService.GetById(id);
                return Content(JsonConvert.SerializeObject(post));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PostsViewModel postsViewModel)
        {
            try
            {
                string host = this._httpContextAccessor.HttpContext.Request.Host.Value;
                string uploads = Path.Combine(this._hostingEnvironment.WebRootPath, "uploadsImgs");
                var post = await _postAppService.Insert(postsViewModel, uploads, host);
                
                var emailServices = new EmailServices();
                var body = emailServices.GetEmailBody(postsViewModel);
                var email = new Email();
                email.To = new List<EmailAddress>();
                email.To.Add(new EmailAddress { Address = "feliperfariasdev@gmail.com", Name = "Felipe Farias" });
                emailServices.SendEmail(email, "(Add Post)", body);
                
                var retorno = new
                {
                    success = true,
                    message = "Cadastrado com sucesso",
                    post = post
                };
                //,new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                return Ok(JsonConvert.SerializeObject(retorno));
            }
            catch (Exception ex)
            {
                var retorno = new
                {
                    success = false,
                    message = ex
                };

                return Ok(JsonConvert.SerializeObject(retorno));
            }
        }

        [HttpPut]
        public async Task<ActionResult> EditAsync([FromForm] PostsViewModel postsViewModel)
        {
            try
            {
                string uploadsImgs = "uploadsImgs";
                string uploads = Path.Combine(this._hostingEnvironment.WebRootPath, uploadsImgs);
                if (postsViewModel.Imagem.Length > 0)
                {
                    string filePath = Path.Combine(uploads, postsViewModel.Imagem.FileName);
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await postsViewModel.Imagem.CopyToAsync(fileStream);
                    }
                    string ImgUrl = this._httpContextAccessor.HttpContext.Request.Host.Value;

                    postsViewModel.ImagemUrl = "https://" + ImgUrl + "/" + uploadsImgs + "/" + postsViewModel.Imagem.FileName;
                }
                DateTime DataCadastro = DateTime.Now;
                postsViewModel.DataCadastro = DataCadastro;
                /* Exemplo sem utilizar o mapper
                var post = new Posts { Titulo = postsViewModel.Titulo, Descricao = postsViewModel.Descricao, DataCadastro = postsViewModel.DataCadastro, ImagemUrl = postsViewModel.ImagemUrl };
                */

                //Exemplo com o mapper
                //var post = this._mapper.Map<Posts>(postsViewModel);

                //applicationDbContext.Posts.Update(post);
                //applicationDbContext.SaveChanges();


                var emailServices = new EmailServices();

                var body = emailServices.GetEmailBody(postsViewModel);
                var email = new Email();
                email.To = new List<EmailAddress>();
                email.To.Add(new EmailAddress { Address = "feliperfariasdev@gmail.com", Name = "Felipe Farias" });
                emailServices.SendEmail(email, "(Update Post)", body);

                var retorno = new
                {
                    success = true,
                    message = "Atualizado com sucesso",
                    //post = post
                };

                return Ok(JsonConvert.SerializeObject(retorno));
            }
            catch (Exception ex)
            {
                var retorno = new
                {
                    success = false,
                    message = ex
                };

                return Ok(JsonConvert.SerializeObject(retorno));
            }
        }

        [HttpDelete]
        [Route("/Posts/{id}")]
        public ActionResult Delete([FromRoute] int id)
        {
            try
            {
                var deletadoComSucesso = _postAppService.Delete(id);
                if (!deletadoComSucesso)
                    return BadRequest("Erro ao realizar exclusão");

                return Ok("post id " + id + " removido com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
