﻿namespace Blog.Domain.Entities
{
    public class Posts
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCadastro { get; set; }
        public string? ImagemUrl { get; set; }
    }
}
