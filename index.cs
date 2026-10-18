<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <title>Jogo do Sobrevivente</title>
  <style>
    * {
      margin: 0;
      padding: 0;
      box-sizing: border-box;
    }

    body {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background-color: #0d1117;
      color: #ffffff;
      font-family: Arial, sans-serif;
      overflow: hidden;
    }

    h1 {
      margin-bottom: 8px;
      font-size: 20px;
    }

    /* Placar de Pontuação na Tela */
    #placar {
      display: flex;
      gap: 15px;
      margin-bottom: 12px;
      background-color: #161b22;
      padding: 8px 16px;
      border-radius: 6px;
      border: 1px solid #30363d;
      font-size: 13px;
    }

    #placar span {
      font-weight: bold;
      color: #38bdf8;
    }

    /* Outer Wrapper para centralizar a tela com escala de Zoom */
    #game-wrapper {
      width: 500px;
      height: 200px;
      position: relative;
      /* ZOOM/ESCALA: Amplia a tela para não ficar muito pequena */
      transform: scale(2.0);
      transform-origin: top center;
      margin-bottom: 210px; /* Compensa o espaço ocupado pelo scale */
    }

    /* Tela Principal do Jogo */
    #game {
      width: 500px;
      height: 200px;
      border: 2px solid #8a2be2;
      position: relative;
      overflow: hidden;
      background-color: #050510;
      cursor: pointer;
    }

    /* Renderização Nítida para Pixel Art */
    #personagem, .zumbi-ent, .explosao, .predio, #chao, #ceu {
      image-rendering: pixelated;
      image-rendering: crisp-edges;
    }

    /* Céu e Chão com Rolagem Infinita */
    #ceu {
      width: 1000px;
      height: 45px;
      position: absolute;
      top: 110px; /* Logo acima do chão */
      left: 0;
      background-image: url("ceu.png");
      background-repeat: repeat-x;
      background-size: contain;
      z-index: 1;
    }

    #chao {
      width: 1000px;
      height: 45px;
      position: absolute;
      top: 155px; /* Chão: de 155px até 200px */
      left: 0;
      background-image: url("chao.png");
      background-repeat: repeat-x;
      background-size: contain;
      z-index: 3;
    }

    /* Container dos Prédios */
    #camada-predios {
      width: 500px;
      height: 200px;
      position: absolute;
      top: 0;
      left: 0;
      z-index: 2;
      pointer-events: none;
    }

    .predio {
      position: absolute;
      background-repeat: no-repeat;
      background-size: contain;
      background-position: bottom left;
    }

    /* Personagem */
    #personagem {
      width: 14px;
      height: 27px;
      position: absolute;
      top: 128px; /* 155 - 27 */
      left: 40px;
      z-index: 10;
      background-image: url("homem_parado.png");
      background-size: contain;
      background-repeat: no-repeat;
    }

    .pular {
      animation: anima_pulo 500ms ease-out;
    }

    @keyframes anima_pulo {
      0%   { top: 128px; }
      50%  { top: 60px;  }
      100% { top: 128px; }
    }

    /* Zumbis Gerados */
    .zumbi-ent {
      position: absolute;
      top: 133px; /* 155 - 22 */
      z-index: 9;
      background-size: contain;
      background-repeat: no-repeat;
    }

    .zumbi-pular {
      animation: anima_pulo 500ms ease-out !important;
    }

    /* Explosão */
    .explosao {
      width: 19px;
      height: 19px;
      position: absolute;
      background-image: url("explosao.png");
      background-size: contain;
      background-repeat: no-repeat;
      z-index: 15;
    }

    .instrucoes {
      text-align: center;
      line-height: 1.5;
      font-size: 13px;
      color: #94a3b8;
    }
  </style>
</head>
<body>

  <h1>Jogo do Sobrevivente</h1>

  <div id="placar">
    <div>Mortos: <span id="qtd-mortos">0</span></div>
    <div>Pulados: <span id="qtd-pulados">0</span></div>
    <div>Evitados: <span id="qtd-evitados">0</span></div>
  </div>

  <div id="game-wrapper">
    <div id="game">
      <div id="ceu"></div>
      <div id="camada-predios"></div>
      <div id="chao"></div>
      <div id="personagem"></div>
    </div>
  </div>

  <div class="instrucoes">
    <p><strong>Controles:</strong></p>
    <p><strong>Pular:</strong> Espaço, W ou Seta para Cima | <strong>Atirar:</strong> Clique na Tela ou Tecla Shift</p>
  </div>

  <script>
    const jogador = document.querySelector("#personagem");
    const game = document.querySelector("#game");
    const camadaPredios = document.querySelector("#camada-predios");
    const elChao = document.querySelector("#chao");
    const elCeu = document.querySelector("#ceu");

    // Elementos de Pontuação
    const txtMortos = document.querySelector("#qtd-mortos");
    const txtPulados = document.querySelector("#qtd-pulados");
    const txtEvitados = document.querySelector("#qtd-evitados");

    let totalMortos = 0;
    let totalPulados = 0;

    function atualizarPlacar() {
      txtMortos.innerText = totalMortos;
      txtPulados.innerText = totalPulados;
      txtEvitados.innerText = totalMortos + totalPulados;
    }

    // Sprites do Personagem
    const SPRITES = {
      jogadorParado: 'url("homem_parado.png")',
      jogadorAtirando: 'url("homem_atirando.png")',
      jogadorPulo: 'url("homem_pulando.png")',
      zumbiPadrao: 'url("zumbi_padrao.png")',
      zumbiEspinhos: 'url("zumbi_espinho.png")',
      zumbiQuarterback: 'url("zumbi_quaterback.png")',
      explosao: 'url("explosao.png")'
    };

    // Novas medidas dos prédios
    const PREDIOS = [
      { id: 'predio_1', largura: 48, altura: 60, img: 'url("predio_1.png")', peso: 3 },
      { id: 'predio_2', largura: 88, altura: 34, img: 'url("predio_2.png")', peso: 3 },
      { id: 'predio_3', largura: 62, altura: 62, img: 'url("predio_3.png")', peso: 1 }, // Aparece menos
      { id: 'predio_4', largura: 44, altura: 44, img: 'url("predio_4.png")', peso: 3 },
      { id: 'predio_5', largura: 24, altura: 60, img: 'url("predio_5.png")', peso: 3 },
      { id: 'predio_6', largura: 48, altura: 32, img: 'url("predio_6.png")', peso: 3 }
    ];

    const TIPOS_ZUMBI = ['padrao', 'espinhas', 'quarterback'];
    
    // Controle de Velocidade e Dificuldade Gradual
    let velocidadeJogo = 1.3; // Início mais suave
    const VELOCIDADE_MAXIMA = 3.2; // Limite máximo para não ficar impossível
    const ACELERACAO = 0.0003; // Incremento contínuo

    let jogoAtivo = true;
    let prediosAtivos = [];
    let zumbisAtivos = [];
    let posChao = 0;
    let posCeu = 0;

    // --- GERENCIAMENTO DOS PRÉDIOS ---

    function sortearPredio() {
      const lista = [];
      PREDIOS.forEach(p => {
        for (let i = 0; i < p.peso; i++) lista.push(p);
      });
      return lista[Math.floor(Math.random() * lista.length)];
    }

    function criarPredio(posX) {
      const pData = sortearPredio();
      const div = document.createElement("div");
      div.className = "predio";
      div.style.width = pData.largura + "px";
      div.style.height = pData.altura + "px";
      div.style.backgroundImage = pData.img;
      div.style.left = posX + "px";
      div.style.top = (155 - pData.altura) + "px";

      camadaPredios.appendChild(div);
      prediosAtivos.push({ elemento: div, x: posX, largura: pData.largura });
    }

    function inicializarPredios() {
      let xAtual = 0;
      while (xAtual < 650) {
        // Espaço mínimo obrigatório de 16px + variação aleatória de espaço maior
        const espacoExtra = Math.random() < 0.35 ? Math.floor(Math.random() * 45) + 15 : 0;
        xAtual += 16 + espacoExtra;
        criarPredio(xAtual);
        const ultimo = prediosAtivos[prediosAtivos.length - 1];
        xAtual += ultimo.largura;
      }
    }

    function atualizarPredios() {
      for (let i = 0; i < prediosAtivos.length; i++) {
        const p = prediosAtivos[i];
        p.x -= velocidadeJogo;
        p.elemento.style.left = p.x + "px";
      }

      if (prediosAtivos.length > 0 && prediosAtivos[0].x + prediosAtivos[0].largura < -20) {
        prediosAtivos[0].elemento.remove();
        prediosAtivos.shift();
      }

      const ultimo = prediosAtivos[prediosAtivos.length - 1];
      if (ultimo && ultimo.x < 500) {
        const espacoExtra = Math.random() < 0.35 ? Math.floor(Math.random() * 45) + 15 : 0;
        let proximoX = ultimo.x + ultimo.largura + 16 + espacoExtra;
        criarPredio(proximoX);
      }
    }

    // --- GERENCIAMENTO DE ZUMBIS (Geração Segura) ---

    function criarZumbi(tipo, posX) {
      const div = document.createElement("div");
      div.className = "zumbi-ent";
      
      let larg = 13;
      let spriteImg = SPRITES.zumbiPadrao;

      if (tipo === 'espinhas') {
        larg = 14;
        spriteImg = SPRITES.zumbiEspinhos;
      } else if (tipo === 'quarterback') {
        larg = 15;
        spriteImg = SPRITES.zumbiQuarterback;
      }

      div.style.width = larg + "px";
      div.style.height = "22px";
      div.style.left = posX + "px";
      div.style.backgroundImage = spriteImg;

      game.appendChild(div);
      zumbisAtivos.push({
        elemento: div,
        x: posX,
        largura: larg,
        tipo: tipo,
        contabilizadoPulo: false
      });
    }

    let timerSpawneZumbi = 0;

    function gerenciarSpawnsZumbis() {
      timerSpawneZumbi -= velocidadeJogo;

      if (timerSpawneZumbi <= 0) {
        // Quantidade aleatória de zumbis (1 ou 2 juntos com distância justa)
        const qtd = Math.random() < 0.3 ? 2 : 1;
        let pX = 520;

        let ultimoTipo = null;

        for (let i = 0; i < qtd; i++) {
          let tipo = TIPOS_ZUMBI[Math.floor(Math.random() * TIPOS_ZUMBI.length)];

          // Evita dois Quarterbacks colados (situação impossível)
          if (ultimoTipo === 'quarterback' && tipo === 'quarterback') {
            tipo = 'padrao';
          }

          criarZumbi(tipo, pX);
          ultimoTipo = tipo;

          // Espaçamento entre zumbis da mesma horda
          pX += Math.floor(Math.random() * 40) + 70;
        }

        // Intervalo aleatório até o próximo grupo
        timerSpawneZumbi = Math.floor(Math.random() * 120) + 140;
      }
    }

    function atualizarZumbis() {
      for (let i = zumbisAtivos.length - 1; i >= 0; i--) {
        const z = zumbisAtivos[i];
        z.x -= velocidadeJogo;
        z.elemento.style.left = z.x + "px";

        // Contabiliza se o jogador pulou e ultrapassou com sucesso
        if (!z.contabilizadoPulo && z.x < 30) {
          z.contabilizadoPulo = true;
          totalPulados++;
          atualizarPlacar();
        }

        // Remove zumbis que saíram da tela
        if (z.x < -30) {
          z.elemento.remove();
          zumbisAtivos.splice(i, 1);
        }
      }
    }

    // --- CONTROLES E AÇÕES ---

    function pular() {
      if (!jogoAtivo) return;
      if (!jogador.classList.contains("pular")) {
        jogador.classList.add("pular");
        jogador.style.backgroundImage = SPRITES.jogadorPulo;
        jogador.style.width = '16px';

        setTimeout(() => {
          jogador.classList.remove("pular");
          jogador.style.backgroundImage = SPRITES.jogadorParado;
          jogador.style.width = '14px';
        }, 500);
      }
    }

    function atirar() {
      if (!jogoAtivo) return;

      jogador.style.backgroundImage = SPRITES.jogadorAtirando;
      jogador.style.width = '16px';

      setTimeout(() => {
        if (!jogador.classList.contains("pular")) {
          jogador.style.backgroundImage = SPRITES.jogadorParado;
          jogador.style.width = '14px';
        }
      }, 200);

      // Testa acerto de tiro nos zumbis mais próximos
      for (let i = 0; i < zumbisAtivos.length; i++) {
        const z = zumbisAtivos[i];

        if (z.x > 30 && z.x < 350) {
          if (z.tipo === 'padrao' || z.tipo === 'espinhas') {
            criarExplosao(z.x, 133);
            z.elemento.remove();
            zumbisAtivos.splice(i, 1);

            totalMortos++;
            atualizarPlacar();
            break;
          }
        }
      }
    }

    function criarExplosao(x, y) {
      const exp = document.createElement("div");
      exp.className = "explosao";
      exp.style.left = x + "px";
      exp.style.top = y + "px";
      game.appendChild(exp);

      setTimeout(() => {
        exp.remove();
      }, 350);
    }

    // Eventos
    window.addEventListener("keydown", (e) => {
      if (e.key === " " || e.key === "w" || e.key === "W" || e.key === "ArrowUp") {
        e.preventDefault();
        pular();
      } else if (e.key === "Shift") {
        e.preventDefault();
        atirar();
      }
    });

    game.addEventListener("click", () => {
      atirar();
    });

    // --- LOOP PRINCIPAL DO JOGO ---

    const loopJogo = setInterval(() => {
      if (!jogoAtivo) return;

      // Aceleração contínua até o limite
      if (velocidadeJogo < VELOCIDADE_MAXIMA) {
        velocidadeJogo += ACELERACAO;
      }

      // Mover Cenários
      posChao -= velocidadeJogo;
      posCeu -= (velocidadeJogo * 0.5);
      if (posChao <= -500) posChao = 0;
      if (posCeu <= -500) posCeu = 0;
      elChao.style.transform = `translateX(${posChao}px)`;
      elCeu.style.transform = `translateX(${posCeu}px)`;

      atualizarPredios();
      gerenciarSpawnsZumbis();
      atualizarZumbis();

      // Checagem de Colisão com Jogador
      const jogTopo = parseInt(window.getComputedStyle(jogador).getPropertyValue("top"));

      for (let i = 0; i < zumbisAtivos.length; i++) {
        const z = zumbisAtivos[i];

        if (z.x < (40 + 14) && (z.x + z.largura) > 40) {
          if (z.tipo === 'espinhas') {
            if (jogTopo < 128) {
              z.elemento.classList.add("zumbi-pular");
              derrota("O Zumbi com Espinhos pulou junto e te matou no ar!");
            } else {
              derrota("Você colidiu com o Zumbi com Espinhos!");
            }
          } else if (z.tipo === 'quarterback') {
            if (jogTopo >= 128) {
              derrota("Você foi atingido pelo Zumbi Quarterback! Você deveria ter pulado.");
            }
          } else if (z.tipo === 'padrao') {
            if (jogTopo >= 128) {
              derrota("Você colidiu com o Zumbi Padrão!");
            }
          }
        }
      }
    }, 10);

    function derrota(mensagem) {
      jogoAtivo = false;
      const totalEvitados = totalMortos + totalPulados;

      setTimeout(() => {
        alert(
          "GAME OVER: " + mensagem + "\n\n" +
          "--- PLACAR FINAL ---\n" +
          "Total de zumbis mortos: " + totalMortos + "\n" +
          "Total de zumbis pulados: " + totalPulados + "\n" +
          "Total de zumbis evitados: " + totalEvitados
        );
        window.location.reload();
      }, 50);
    }

    // Inicialização
    inicializarPredios();
    atualizarPlacar();
  </script>
</body>
</html>