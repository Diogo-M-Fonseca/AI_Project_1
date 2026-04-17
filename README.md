# Simulação de Colónia Marciana com Agentes Autónomos e Gestão de Incidentes (Projecto 1 Disciplina AI)

## Autores

- Diogo Meira Fonseca - nº a22402652
- joão Monteiro - nº a22302592

## Distribuição do trabalho

- Implementação dos Tripulantes: Diogo Fonseca
- Implementação dos Robos:       Diogo Fonseca
- Implementação dos Incidentes:  Diogo Fonseca
- Implementação dos Módulos:     Diogo Fonseca
- Relatório:                     Diogo Fonseca

## Introdução

Como o nome indica este projecto tenta simular uma colónia marciana com agentes autonomos, estes agentes são divididos em duas categorias Tripulantes e robos, tendo cada uma dessas categorias necessidades e objectivos diferentes, essas necessidades e objectivos correspondem a modulos especificos que podem ser afetados por incidentes, estes complicam ou impossiblitam concluir essas necessidades e objectivos.

## Metodologia

Esta simulação foi desenvolvida em Unity 3d com os assets 3d nativos do unity (planes, cylinders, spheres, etc).

Esta simulação tem um sistema de Módulos, os módulos representam as salas da colónia marciana onde os agentes executam as suas tarefas, ou seja definem tanto o espaço navegável quanto as regras das ações dos agentes

O projecto tem um Enum com os tipos de modulo esses tipos definem as interações que os agentes conseguem ter neles, esses tipos são: Habitat, Laboratory, GreenHouse, Storage, Technical e Escape

O projecto tem também um enum com cada tipo de estado que um módulo pode estar dependendo do incidente que o afeta, esse estado é mudado quando um incidente é ativado ou desativado em cada modulo, esses estados são: Normal, Fire, NoOxigen, NoPower

Eu implementei os modulos criando uma classe chamada Module onde se define o tipo e estado de cada modulo, tenta se defenir uma capacidade de agentes maxima, verifica-se entrada e saida de agentes, faz-se atribuição de robos (garante-se que apenas um robo por vez pode arranjar o mesmo modulo), tenta dar proteção contra incidentes aos modulos de escape, adiciona um cooldown quando arranjado que impede de ser afetado por um incidente imediatamente após ser arranjado, 

A navegação dos agentes é feita marioritariamente com NavMesh para garantir que os agentes evitariam paredes, limites e obstáculos, esse sistema facilita bastante o trabalho pois permite me utilizar de algoritmos de navegação pre feitos do unity em vez de ter que desenvolver os meus próprios. 

Os Agentes são divididos em duas categorias: Tripulantes e robos

Os tripulantes têm como necessidades dinamicas Energia, Recursos e "Necessidade de verde" estas necessidades começam com valores fixos e vão decrescendo continuamente a velocidades diferentes, o tripulante verifica o valor destas necessidades e age de acordo, dando sempre prioridade á energia, essas ações consistem de se locomover para um modulo especifico e reabastecer essa necessidade, cada necessidade corresponde a um módulo, Energia só consegue ser reabastecida nas habitações, Recursos só podem ser reabastecidos no armazém, necessidade de verde só pode ser reabastecida na estufa e caso nenhuma necessidade esteja baixa o suficiente o tripulante irá para um laboratório trabalhar, caso um numero grande de incidentes esteja ativo o tripulante começa a evacuar, a ação de evacuação consiste de se movimentar para o modulo de saida, após chegar a esse módulo o GameObject do tripulante é desativado

Os tripulantes têm um valor de Vida que ao passar por um módulo com o incidente Fogo ou Falta de Oxigénio desce, se chegar a zero o GameObject do tripulante é destruido

Eu queria ter implementado um sistema que interagia com o navmesh e faria o tripulante evitar com diferentes niveis de sériedade modulos com incidentes, mas infelizmente não consegui

O sistema de Ai do tripulante é um FSM (finite state machine)
que consiste do seguinte:

```mermaid
stateDiagram-v2
    [*] --> Idle

    Idle --> Moving : PickNextTask()

    Moving --> Working : Arrive at non-Habitat module
    Moving --> Resting : Arrive at Habitat module
    Moving --> Idle : Target invalid / interrupted

    Working --> Idle : Task completed

    Resting --> Idle : Energy restored

    Idle --> RespondingToIncident : Detect incident (fire / oxygen leak)

    RespondingToIncident --> Moving : Move to safe module

    Idle --> Evacuating : Evacuation triggered
    Moving --> Evacuating : Evacuation triggered
    Working --> Evacuating : Evacuation triggered
    Resting --> Evacuating : Evacuation triggered
    RespondingToIncident --> Evacuating : Evacuation triggered

    Evacuating --> [*] : Reaches escape capsule / exits base
```

O script dos tripulantes foi um dos mais complicados de trabalhar neste projecto, acho que deveria ter dividido as suas funções em diferentes scripts para evitar este script enorme que á minima mudança deixa de funciona, admito também que poderia ter tido mais atenção aos principios SOLID e/ou utilizado de um design patter diferente neste projecto para facilitar o meu trabalho

Os robos têm apenas uma necessidade dinamica, Bateria, caso o valor dessa necessidade seja demasiado pequeno o robo irá mover-se para um modulo técnico e irá carregar a bateria continuamente, no entantanto as ações dos robos não se limitam a reabastecer essa necessidade dinamica, os robos analisam os modulos da simulação e caso encontrem algum com icidente ativo movem se até esse modulo e arranjam-no retirando assim o incidente

Os robos têm um sistema que garante que apenas um robo irá arranjar um módulo de cada vez para evitar mais que um robo arranje o mesmo incidente no mesmo módulo

O sistema de Ai do tripulante é um FSM (finite state machine)
que consiste do seguinte:

```mermaid
stateDiagram-v2
    [*] --> Idle

    Idle --> Moving : PickTask()

    Moving --> Idle : Arrives at charging station (battery OK)
    Moving --> RespondingToIncident : Target module in incident

    Idle --> Charging : Battery < threshold (FindChargingStation)
    Moving --> Charging : Battery < threshold

    Charging --> Idle : Battery full

    Idle --> RespondingToIncident : Incident detected
    Charging --> RespondingToIncident : Incident detected
    Moving --> RespondingToIncident : Incident detected

    RespondingToIncident --> Moving : Navigate to affected module
    RespondingToIncident --> Repair : Arrive at incident module

    Repair --> Idle : Repair completed

    RespondingToIncident --> Idle : Target lost / invalid
```

Os robos não têm qualquer sistema de vida e mesmo em situação de evacuação eles continuam a trabalhar normalmente para tentar conter os incidentes

O script dos robos já foi menos complicado que o dos tripulantes justamente por ser uma versão alterada do mesmo script, o problema disso foi muitos dos problemas de design de código de um script passou para o outro, efetivamente em vez de fazer um script para robos fiz um script de um tripulante robotizado


O sistema de incidentes desta simulação introdus eventos dinamicos que afetam a maneira como os agentes fazem as suas decisões

No enunciado pede para que os incidentes afetem tanto modulos quanto corredores mas infelizmente neste projecto apenas consegui implementar nos módulos 

Eu implementei o sistema através de duas classes: IncidentManager e Incident

Incident é uma classe que guarda as informações essenciais de um incidente o seu tipo a sua origem e á quanto tempo está ativo

O IncidentManager é um singleton que controla os efeitos e propagação dos incidentes, ele tem uma lista de todos os incidentes ativos e gere a criação de incidentes a sua propagação e a condição de evacuação, um incidente aleatório é criado num modulo aleatório a cada 15 segundos, inicialmente eu queria que fosse em intrevalos de tempos aleatórios mas é mais facil testar certas coisas se for em intrevalos de tempo constantes.

Os agentes tentam sempre agir de acordo com os incidentes ativos

Aqui se segue um flowchart do projecto

```mermaid
graph TD

T[Tripulante]
R[Robo]
M[Module]
IM[IncidentManager]
I[Incident]

T -->|move and task selection| M
R -->|repair and charging| M

M -->|state normal or danger| T
M -->|state normal or danger| R

IM -->|creates incident| I
IM -->|applies incident| M
IM -->|spreads incident| M

I -->|updates time| IM

IM -->|evacuation signal| T
IM -->|evacuation signal| R
```

