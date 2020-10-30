using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
namespace nig
{
    struct Produto
    {
        public double Price { get; set; }
        public string Nome { get; set; }
    }

    enum StatusPedido
    {
        NaoEnviado,
        Enviado,
        Entregue
    }

    struct Pedido
    {
        public DateTime Data { get; set; }
        public string NomeDoCliente { get; set; }

        public StatusPedido Status { get; set; }
        public Produto Produto { get; set; }

        public void Despachar()
        {
            this.Status = StatusPedido.Enviado;
        }

        public string StatusEnvio()
        {
            switch (this.Status)
            {
                case StatusPedido.NaoEnviado: return "Não enviado";
                default: return Status.ToString();
            }
        }
    }

    class Loja
    {
        public Loja()
        {
            _estoque = new List<Produto>();
            _pedidos = new List<Pedido>();
        }
        public Loja(string nomeLoja)
        {
            _nome = nomeLoja;
            _estoque = new List<Produto>();
            _pedidos = new List<Pedido>();
        }

        public Loja(string nomeLoja, List<Produto> estoque, List<Pedido> pedidos)
        {
            _nome = nomeLoja;
            _estoque = estoque;
            _pedidos = pedidos;
        }

        public void AdicionarProduto(Produto p)
        {
            _estoque.Add(p);
        }

        public bool ContemProduto(string nome)
        {
            foreach (var p in _estoque)
            {
                if (p.Nome == nome)
                {
                    return true;
                }
            }
            return false;
        }

        public int ContarProduto(string nome)
        {
            return _estoque.Count(x => x.Nome == nome);
        }

        public void MostrarProdutos()
        {
            if (_estoque.Count == 0)
            {
                Console.WriteLine("Não há produtos para exibir.");
            }
            else
            {
                foreach (var p in _estoque)
                {
                    Console.WriteLine($"{ p.Nome } - R$ { p.Price }");
                }
            }
        }

        public bool ContemPedidosDeCliente(string nomeCliente)
        {
            foreach (var p in _pedidos)
            {
                if (p.NomeDoCliente == nomeCliente)
                {
                    return true;
                }
            }

            return false;
        }
        public void MostrarPedidos(string nomeCliente)
        {
            if (!ContemPedidosDeCliente(nomeCliente))
            {
                return;
            }

            var pedidosCliente = _pedidos.Where(x => x.NomeDoCliente == nomeCliente);
            foreach (var ped in pedidosCliente)
            {
                Console.WriteLine($"[{ ped.Data.ToShortTimeString() }] { ped.Produto.Nome } - R$ { ped.Produto.Price } - { ped.StatusEnvio() }");
            }
        }

        public bool ContemPedidos()
        {
            return _pedidos.Count != 0;
        }
        public void MostrarVendas()
        {
            if (!ContemPedidos())
            {
                Console.WriteLine("Nenhum item foi vendido ainda.");
                return;
            }

            var totalVendas = 0.0;
            foreach (var p in _pedidos)
            {
                Console.WriteLine($"Vendido {p.Produto.Nome} por R$ { p.Produto.Price } às { p.Data.ToShortTimeString() }");
                totalVendas += p.Produto.Price;
            }
            Console.WriteLine($"Total em vendas: R$ { totalVendas }");
        }

        public void DespacharProdutos()
        {
            foreach (var p in _pedidos)
            {
                if (p.Status == StatusPedido.NaoEnviado)
                {
                    p.Despachar();
                }
            }
        }

        public bool ComprarProduto(string nomeCliente, string pedido)
        {
            if (!ContemProduto(pedido))
            {
                return false;
            }

            var produtoAdd = _estoque.FirstOrDefault(x => x.Nome == pedido);
            _pedidos.Add(new Pedido
            {
                NomeDoCliente = nomeCliente,
                Data = DateTime.Now,
                Produto = produtoAdd,
                Status = StatusPedido.NaoEnviado
            });
            _estoque.Remove(produtoAdd);

            return true;
        }

        public string NomeDaLoja()
        {
            return _nome;
        }

        [JsonProperty("NomeDaLoja")]
        private string _nome;

        [JsonProperty("Pedidos")]
        private List<Pedido> _pedidos;
        [JsonProperty("Estoque")]
        private List<Produto> _estoque;
    }

    class LojaAdmin
    {
        public LojaAdmin()
        {
            if (File.Exists("db.json"))
            {
                var txt = File.ReadAllText("db.json");
                _lojas = JsonConvert.DeserializeObject<List<Loja>>(txt);
            }
            else
            {
                _lojas = new List<Loja>();
            }
        }

        public bool AdicionarLoja(string nomeLoja)
        {
            var loja = _lojas.FirstOrDefault(x => x.NomeDaLoja() == nomeLoja);
            if (loja == null)
            {
                _lojas.Add(new Loja(nomeLoja));
                return true; // loja com esse nome ja existe
            }

            return false;
        }

        public void MostrarProdutos(string lojaEspecifica = null)
        {
            if (lojaEspecifica != null)
            {
                var loja = _lojas.FirstOrDefault(x => x.NomeDaLoja() == lojaEspecifica);
                if (loja == null)
                {
                    Console.WriteLine("Loja não existe!");
                }
                else
                {
                    loja.MostrarProdutos();
                }
            }
            else
            {
                foreach (var loja in _lojas)
                {
                    Console.WriteLine($"Nome da loja: { loja.NomeDaLoja() }\n----------------------------");
                    loja.MostrarProdutos();
                }
            }
        }

        public void MostrarPedidos(string nomeCliente)
        {
            foreach (var loja in _lojas)
            {
                if (loja.ContemPedidosDeCliente(nomeCliente))
                {
                    Console.WriteLine($"Nome da loja: { loja.NomeDaLoja() }\n----------------------------");
                    loja.MostrarPedidos(nomeCliente);
                }

            }
        }

        public void MostrarVendas()
        {
            foreach (var loja in _lojas)
            {
                Console.WriteLine($"Nome da loja: { loja.NomeDaLoja() }\n----------------------------");
                loja.MostrarVendas();
            }
        }

        public void DespacharProdutos()
        {
            foreach (var loja in _lojas)
            {
                loja.DespacharProdutos();
            }
        }

        public bool ContemProduto(string nome)
        {
            return _lojas.FirstOrDefault(x => x.ContemProduto(nome)) != null;
        }
        public void AdicionarProduto(Produto p)
        {
            // adicionar produto na primeira loja que nao tem o produto
            var lojaSemProduto = _lojas.FirstOrDefault(x => !x.ContemProduto(p.Nome));
            if (lojaSemProduto != null)
            {
                lojaSemProduto.AdicionarProduto(p);
                return;
            }

            // todas as lojas tem o produto, adicionar na loja com menos quantidade do produto
            var lojaMenosProduto = _lojas.OrderBy(x => x.ContarProduto(p.Nome)).First();
            lojaMenosProduto.AdicionarProduto(p);
        }

        public bool ComprarProduto(string nomeCliente, string nomeProduto)
        {
            return _lojas.FirstOrDefault(x => x.ContemProduto(nomeProduto)).ComprarProduto(nomeCliente, nomeProduto);
        }

        public void Salvar()
        {
            var txt = JsonConvert.SerializeObject(_lojas, Formatting.Indented);
            File.WriteAllText("db.json", txt);
        }

        public void VerLojas()
        {
            _lojas.ForEach(x => Console.WriteLine($"Loja: { x.NomeDaLoja() }"));
        }

        List<Loja> _lojas;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Qual o tipo de usuário?\n1 - Gerente\n2 - Cliente\n3 - Sair");

            bool isClient = false;
            if (!int.TryParse(Console.ReadLine(), out int n) || (n > 2))
            {
                Console.WriteLine("Saindo...");
                return;
            }
            else
            {
                isClient = n == 2;
            }

            Console.WriteLine($"Bem-vindo { (isClient ? "Cliente" : "Gerente") }");
            var lojaMestra = new LojaAdmin();

            bool loop = true;
            while (loop)
            {
                if (isClient)
                {
                    Console.WriteLine("--------------------------");
                    Console.WriteLine("1 - Ver lojas");
                    Console.WriteLine("2 - Listar produtos");
                    Console.WriteLine("3 - Comprar produto");
                    Console.WriteLine("4 - Status dos pedidos");
                    Console.WriteLine("5 - Sair");
                    Console.WriteLine("--------------------------");

                    Console.Write("> ");
                    int opt = 0;
                    if (!int.TryParse(Console.ReadLine(), out opt))
                    {
                        break;
                    }

                    switch (opt)
                    {
                        case 1:
                            break;

                        case 2:
                            lojaMestra.MostrarProdutos(null);
                            break;

                        case 3:
                            {
                                Console.Write("Nome do cliente: ");
                                string nomeCliente = Console.ReadLine();

                                Console.Write("Nome do produto: ");
                                string nomeProduto = Console.ReadLine();

                                if (!lojaMestra.ContemProduto(nomeProduto))
                                {
                                    Console.WriteLine("Produto não existe!");
                                    continue;
                                }

                                if (lojaMestra.ComprarProduto(nomeCliente, nomeProduto))
                                {
                                    Console.WriteLine("Pedido feito com sucesso!");
                                }
                                else
                                {
                                    Console.WriteLine("Produto não encontrado.");
                                }
                                break;
                            }

                        case 4:
                            {
                                Console.Write("Nome do cliente: ");
                                string nomeCliente = Console.ReadLine();

                                lojaMestra.MostrarPedidos(nomeCliente);
                                break;
                            }

                        case 5:
                            {
                                loop = false;
                                lojaMestra.Salvar();
                                break;
                            }
                    }
                }

                else
                {
                    Console.WriteLine("--------------------------");
                    Console.WriteLine("1 - Ver lojas");
                    Console.WriteLine("2 - Listar produtos");
                    Console.WriteLine("3 - Adicionar produto");
                    Console.WriteLine("4 - Resumo de vendas");
                    Console.WriteLine("5 - Despachar produtos não enviados");
                    Console.WriteLine("6 - Adicionar loja");
                    Console.WriteLine("7 - Sair");
                    Console.WriteLine("--------------------------");
                    Console.Write("> ");

                    int opt = 0;
                    if (!int.TryParse(Console.ReadLine(), out opt))
                    {
                        break;
                    }

                    switch (opt)
                    {
                        case 1:
                            lojaMestra.VerLojas();
                            break;

                        case 2:
                            lojaMestra.MostrarProdutos();
                            break;

                        case 3:
                            {
                                Console.Write("Nome do produto: ");
                                string nomeProduto = Console.ReadLine();

                                Console.Write("Preço: ");
                                double price = double.Parse(Console.ReadLine());

                                lojaMestra.AdicionarProduto(new Produto
                                {
                                    Nome = nomeProduto,
                                    Price = price
                                });
                                break;
                            }

                        case 4:
                            lojaMestra.MostrarVendas();
                            break;

                        case 5:
                            lojaMestra.DespacharProdutos();
                            Console.WriteLine("Todos os produtos foram despachados!");
                            break;

                        case 6:
                            {
                                Console.Write("Nome da loja: ");
                                string nomeLoja = Console.ReadLine();
                                if (lojaMestra.AdicionarLoja(nomeLoja))
                                {
                                    Console.WriteLine("Loja adicionada com sucesso!");
                                }
                                else
                                {
                                    Console.WriteLine("Uma loja com este nome já existe.");
                                }
                                break;
                            }

                        case 7:
                            {
                                lojaMestra.Salvar();
                                loop = false;
                                break;
                            }
                    }
                }
            }
        }
    }
}
