namespace TrabalhoPOO{
    #region 4. Interface & 5. Polimorfismo 
    public interface IPagamentoStrategy{
        void Processar(decimal valor);
    }
    #endregion
    #region 2. Hierarquia de Herança e 3.Classe Abstrata
    public abstract class PagamentoBase : IPagamentoStrategy{
        public DateTime DataTransacao { get; set; } = DateTime.Now;
        public abstract void Processar(decimal valor); 
    }
    public class PagamentoEletronico : PagamentoBase{
        public string CodigoAutorizacao { get; set; }
        public override void Processar(decimal valor){
            CodigoAutorizacao = Guid.NewGuid().ToString().Substring(0, 8);
            Console.WriteLine($"[Eletrônico] Pré-autorizando transação no valor de R${valor}...");
        }
    }
    public class PagamentoPix : PagamentoEletronico {
        public string ChavePix { get; set; }
        public override void Processar(decimal valor){
            base.Processar(valor); // Chama o nível 2
            Console.WriteLine($"[PIX] Sucesso! R${valor} transferido para a chave: {ChavePix}. Aut: {CodigoAutorizacao}");
        }
    }
    #endregion

    #region 6. Validação e Tratamento de Exceções
    // Atende ao requisito 6 (Exceção customizada)
    public class NegocioException : Exception
    {
        public NegocioException(string mensagem) : base(mensagem) { }
    }
    #endregion

    #region Entidades do Domínio
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
    }

    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
    }

    public class Pedido
    {
        public int Id { get; set; }
        public Cliente Cliente { get; set; }
        public List<Produto> Itens { get; set; } = new List<Produto>();
        public decimal Total => Itens.Sum(p => p.Preco);
        
        // 5. Operação Polimórfica (Recebe a Interface/Strategy)
        public void FinalizarPedido(IPagamentoStrategy meioPagamento)
        {
            if (!Itens.Any())
                throw new NegocioException("Não é possível finalizar um pedido sem itens!");

            Console.WriteLine($"Finalizando Pedido #{Id} para {Cliente.Nome}. Total: R${Total}");
            meioPagamento.Processar(Total); // Chamada polimórfica
        }
    }
    #endregion

    #region 7. Repositórios (Simulação de 3 CRUDs em Memória)
    // CRUD 1: Clientes
    public class ClienteRepository
    {
        private readonly List<Cliente> _clientes = new List<Cliente>();
        private int _idContador = 1;

        public void Criar(Cliente c) 
        { 
            if (string.IsNullOrWhiteSpace(c.Nome)) throw new NegocioException("Nome inválido!");
            c.Id = _idContador++; _clientes.Add(c); 
        }
        public List<Cliente> Listar() => _clientes;
        public void Atualizar(int id, string novoNome) 
        {
            var c = _clientes.FirstOrDefault(x => x.Id == id) ?? throw new NegocioException("Cliente não encontrado!");
            c.Nome = novoNome;
        }
        public void Deletar(int id) => _clientes.RemoveAll(x => x.Id == id);
    }

    // CRUD 2: Produtos
    public class ProdutoRepository
    {
        private readonly List<Produto> _produtos = new List<Produto>();
        private int _idContador = 1;

        public void Criar(Produto p) 
        { 
            if (p.Preco <= 0) throw new NegocioException("O preço deve ser maior que zero!");
            p.Id = _idContador++; _produtos.Add(p); 
        }
        public List<Produto> Listar() => _produtos;
        public void Atualizar(int id, decimal novoPreco)
        {
            var p = _produtos.FirstOrDefault(x => x.Id == id) ?? throw new NegocioException("Produto não encontrado!");
            p.Preco = novoPreco;
        }
        public void Deletar(int id) => _produtos.RemoveAll(x => x.Id == id);
    }

    // CRUD 3: Pedidos
    public class PedidoRepository
    {
        private readonly List<Pedido> _pedidos = new List<Pedido>();
        private int _idContador = 1;

        public void Criar(Pedido p) { p.Id = _idContador++; _pedidos.Add(p); }
        public List<Pedido> Listar() => _pedidos;
        public void Atualizar(int id, Produto novoProduto)
        {
            var p = _pedidos.FirstOrDefault(x => x.Id == id) ?? throw new NegocioException("Pedido não encontrado!");
            p.Itens.Add(novoProduto);
        }
        public void Deletar(int id) => _pedidos.RemoveAll(x => x.Id == id);
    }
    #endregion

    #region Programa Principal (Console)
    class Program
    {
        static void Main(string[] args)
        {
            // Instanciando Repositórios (Interfaces de CRUD)
            var repoCliente = new ClienteRepository();
            var repoProduto = new ProdutoRepository();
            var repoPedido = new PedidoRepository();

            Console.WriteLine("--- EXECUTANDO TESTE DO SISTEMA ---");

            try
            {
                // 1. Populando dados (Executando as operações de CRIAR dos CRUDs)
                var cliente = new Cliente { Nome = "João Silva", Cpf = "123.456.789-00" };
                repoCliente.Criar(cliente);

                var produto1 = new Produto { Nome = "Teclado Mecânico", Preco = 350.00m };
                var produto2 = new Produto { Nome = "Mouse Gamer", Preco = 150.00m };
                repoProduto.Criar(produto1);
                repoProduto.Criar(produto2);

                // 2. Criando o Pedido
                var pedido = new Pedido { Cliente = cliente };
                pedido.Itens.Add(produto1);
                pedido.Itens.Add(produto2);
                repoPedido.Criar(pedido);

                // 3. Executando Polimorfismo usando a estratégia do Nível 3 da herança
                IPagamentoStrategy formaPagto = new PagamentoPix { ChavePix = "joao@email.com" };
                pedido.FinalizarPedido(formaPagto);

                // 4. Testando o mecanismo de Validação e Exceção (Descomente para testar)
                // var produtoInvalido = new Produto { Nome = "Erro", Preco = -10m };
                // repoProduto.Criar(produtoInvalido);

                Console.WriteLine("\nProcesso concluído com sucesso!");
            }
            catch (NegocioException ex)
            {
                Console.WriteLine($"\n[ERRO DE VALIDAÇÃO]: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERRO INESPERADO]: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
    #endregion
}