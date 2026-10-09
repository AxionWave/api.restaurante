using Orion.Application.Salao;

namespace Orion.Infrastructure.Persistence.Entities;

public class Ambiente
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public List<Mesa> Mesas { get; set; } = [];
}

public class Mesa
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public long AmbienteId { get; set; }
    public Ambiente? Ambiente { get; set; }
    public string Rotulo { get; set; } = string.Empty;
    public int LugaresPadrao { get; set; } = 4;
    public FormaMesa Forma { get; set; } = FormaMesa.Retangular;
    public SituacaoMesa Situacao { get; set; } = SituacaoMesa.Ativa;
    public long? AtendimentoAbertoId { get; set; }
}

public class CartaCategoria
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public List<CartaItem> Itens { get; set; } = [];
}

public class CartaItem
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public long CategoriaId { get; set; }
    public CartaCategoria? Categoria { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public DestinoPedido Destino { get; set; } = DestinoPedido.Cozinha;
    public bool Ativo { get; set; } = true;
}

public class Atendimento
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public long MesaId { get; set; }
    public Mesa? Mesa { get; set; }
    public StatusAtendimento Status { get; set; } = StatusAtendimento.Aberta;
    public int AbertoPorUsuarioId { get; set; }
    public string AbertoPorNome { get; set; } = string.Empty;
    public DateTime AbertoEm { get; set; } = DateTime.UtcNow;
    public DateTime? FechadoEm { get; set; }
    public ModoFechamento? ModoFechamento { get; set; }
    public long? AnfitriaoLugarId { get; set; }
    public List<AtendimentoLugar> Lugares { get; set; } = [];
    public List<ComandaItem> Itens { get; set; } = [];
    public List<GrupoCobranca> Grupos { get; set; } = [];
}

public class GrupoCobranca
{
    public long Id { get; set; }
    public long AtendimentoId { get; set; }
    public Atendimento? Atendimento { get; set; }
    public ModoGrupoCobranca Modo { get; set; } = ModoGrupoCobranca.Junto;
    public List<GrupoCobrancaLugar> Lugares { get; set; } = [];
}

public class GrupoCobrancaLugar
{
    public long GrupoCobrancaId { get; set; }
    public GrupoCobranca? Grupo { get; set; }
    public long LugarId { get; set; }
    public AtendimentoLugar? Lugar { get; set; }
}

public class AtendimentoLugar
{
    public long Id { get; set; }
    public long AtendimentoId { get; set; }
    public Atendimento? Atendimento { get; set; }
    public int Ordem { get; set; }
    public string? NomeCliente { get; set; }
    public bool Ocupado { get; set; }
    public bool ContaFechada { get; set; }
}

public class ComandaItem
{
    public long Id { get; set; }
    public long AtendimentoId { get; set; }
    public Atendimento? Atendimento { get; set; }
    public long? LugarId { get; set; }
    public long? CartaItemId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; } = 1;
    public string? Observacao { get; set; }
    public DestinoPedido Destino { get; set; }
    public StatusItem Status { get; set; } = StatusItem.Lancado;
    public int LancadoPorUsuarioId { get; set; }
    public string LancadoPorNome { get; set; } = string.Empty;
    public DateTime LancadoEm { get; set; } = DateTime.UtcNow;
    public int? CanceladoPorUsuarioId { get; set; }
    public string? CanceladoPorNome { get; set; }
    public DateTime? CanceladoEm { get; set; }
    public string? MotivoCancelamento { get; set; }
}
