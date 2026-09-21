namespace Orion.Application.Estoque;

/// <summary>ENTRADA soma ao saldo; AJUSTE define o saldo (contagem de inventário).</summary>
public enum TipoEntrada { Entrada, Ajuste }

/// <summary>Motivo de negócio — mapeia para <c>OrigemMovimentacao</c> do Core.</summary>
public enum OrigemEntrada { Compra, Manual, Inventario, Devolucao }

public enum StatusEntrada { Rascunho, Confirmada, Cancelada }

/// <summary>Como a entrada foi criada (informativo).</summary>
public enum OrigemCadastro { Manual, Scanner, XmlNfe, ApiFiscal }

public enum StatusVinculoItem { Pendente, Vinculado, ProdutoNovo }
