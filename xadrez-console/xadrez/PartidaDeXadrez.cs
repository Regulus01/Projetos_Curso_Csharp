﻿using tabuleiro;
using tabuleiro.enums;
using System.Collections.Generic;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro Tab { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool Terminada { get; private set; }
        private readonly HashSet<Peca> pecas;
        private readonly HashSet<Peca> capturadas;
        public bool Xeque { get; private set; }
        public PartidaDeXadrez()
        {
            Tab = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Branca;
            Terminada = false;
            Xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tab.RetiraPeca(origem);
            p.IncrementarQteMovimentos();
            Peca pecaCapturada = Tab.RetiraPeca(destino);
            Tab.ColocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            return pecaCapturada;
        }

        public void DesfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = Tab.RetiraPeca(destino);
            p.DecrementarQteMovimentos();
            if (pecaCapturada != null)
            {
                Tab.ColocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            Tab.ColocarPeca(p, origem);

        }
        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = ExecutaMovimento(origem, destino);

            if (EstaEmXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque");
            }
            if (EstaEmXeque(Adversaria(JogadorAtual)))
            {
                Xeque = true;
            }
            else
            {
                Xeque = false;
            }

            if (TesteXequeMate(Adversaria(JogadorAtual)))
            {
                Terminada = true;
            }
            else
            {
                Turno++;
                MudaJogador();
            }
     
        }


        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            //metodo que testa se a posicao é valida(se existe peça na prosição).
            //quando null não existe peça e irá lançar uma exeção
            if (Tab.Peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            //verifica se a peça movida é referente a cor da peça do turno.
            if (JogadorAtual != Tab.Peca(pos).Cor)
            {
                throw new TabuleiroException("A peça na posição escolhida, não é sua!");
            }
            //verifica se a peça não está bloqueada(se a alguma peça da mesma cor na frente)
            //em relação a seus movimentos possíveis
            if (!Tab.Peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!Tab.Peca(origem).PodeMoverPara(destino))
            {
                throw new TabuleiroException("Posicão de destino inválida!");
            }
        }
        private void MudaJogador()
        {
            if (JogadorAtual == Cor.Branca)
            {
                JogadorAtual = Cor.Preta;
            }
            else
            {
                JogadorAtual = Cor.Branca;
            }
        }
        public HashSet<Peca> PecasCapturadas(Cor cor)
        {
            //retorna todas as peças de um cor especificada(informada por parametro)
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca item in capturadas)
            {
                if (item.Cor == cor)
                {
                    aux.Add(item);
                }
            }

            return aux;
        }
        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            //Pecas em jogo de uma determinada cor
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca item in pecas)
            {
                if (item.Cor == cor)
                {
                    aux.Add(item);
                }
            }
            aux.ExceptWith(PecasCapturadas(cor));
            return aux;
        }

        private static Cor Adversaria(Cor cor)
        {
            //verificar se static funciona
            if (cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }

        private Peca Rei(Cor cor)
        {
            foreach (Peca item in PecasEmJogo(cor))
            {
                // is = teste se uma veriavel do tipo da superclasse,
                // é uma instancia de alguma subclasse utiliza o "is"
                if (item is Rei)
                {
                    return item;
                }
            }
            return null;
        }

        public bool EstaEmXeque(Cor cor)
        {
            Peca R = Rei(cor);
            if (R == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + "no tabuleiro!");
            }
            foreach (Peca item in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] mat = item.MovimentosPossiveis();
                if (mat[R.Posicao.Linha, R.Posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool TesteXequeMate(Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca item in PecasEmJogo(cor))
            {
                bool[,] mat = item.MovimentosPossiveis();
                for (int i = 0; i < Tab.Linhas; i++)
                {
                    for (int j = 0; j < Tab.Colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = item.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = ExecutaMovimento(origem, destino);
                            bool testeXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }

                        }
                    }
                }
            }

            return true;
        }

        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            //alem de instanciar a peca agora guarda a peça em um conjunto(pecas)
            Tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            pecas.Add(peca);
        }
        private void ColocarPecas()
        {
            ColocarNovaPeca('c', 1, new Torre(Tab, Cor.Branca));
        
            ColocarNovaPeca('h', 7, new Torre(Tab, Cor.Branca));
      
            ColocarNovaPeca('d', 1, new Rei(Tab, Cor.Branca));


            ColocarNovaPeca('b', 8, new Torre(Tab, Cor.Preta));
            ColocarNovaPeca('a', 8, new Rei(Tab, Cor.Preta));

        }
    }
}
