using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarCategorias;

public class ListarCategoriasQueryHandler
{
    private readonly ICategoriaRepository _categoriaRepository;

    public ListarCategoriasQueryHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<List<CategoriaListItemDto>> Handle(ListarCategoriasQuery query, CancellationToken cancellationToken)
    {
        var categorias = await _categoriaRepository.ObtenerTodasAsync(cancellationToken);
        return categorias.Select(c => new CategoriaListItemDto { Id = c.Id, Nombre = c.Nombre }).ToList();
    }
}