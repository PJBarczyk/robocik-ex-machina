# Robocik ex Machina

Robocik ex Machina to gra oraz jednocześnie fundament pod symulator i narzędzie wizualizacyjne - wykonany jako zadanie rekrutacyjne nr 1 KN Robocik.

Zrealizowany w Unity 2021.2.3f, wykorzystuje wiele nowych i aktywnie rozwijanych narzędzi jak [UI Toolkit], [Shader Graph] czy [Input System].

## Moja interpretacja

Do zadania przystąpiłem z niewielkim doświadczeniem. W ciągu dni jednak rozmyta wizja uformowała się w praktycznie kompletną choć prostą grę. Od początku wiedziałem, że nie zrobię symulatora z algorytmem autonomicznej eksploracji, więc postanowiłem stworzyć pod postacią gry prezentację możliwości i moich, i samego Unity, które mogą znaleźć zastosowanie w prawdziwym programie, którego koło będzie wykorzystywało.

W grze użytkownik może wygenerować topologię na podstawie przykładowych map bądź załadować obrazek z komputera, a następnie dostosować parametry jak powierzchnia i głębokość oraz ilość celów do znalezienia. Po przejściu do gry, użytkownik steruje dronem przy pomocy klawiatury i stara się odnaleźć wszystkie z celów, starając uniknąć zderzenia z dnem.

## Zastosowane rozwiązania

- **UI Toolkit** -- Powstały niedawno i intensywnie rozwijany system UI wraz z graficznym edytorem pozwala na konstruowanie interfejsu w sposób analogiczny to tworzenia front-endu witryn. Struktura oparta na flexbox'ach oraz arkusze stylów praktycznie tożsamych CSS przyśpiesza tworzenie UI czy to w wbudowanym edytorze UI Buidler czy proceduralnie w skryptach C#.
- **Topologia na bazie heightmapy** -- Nie mając doświadczenia w algorytmach generowania proceduralnego terenu, postawiłem na możliwość zaimportowania dowolnego obrazka jako mapy wysokości. Początkowo tymczasowe rozwiązanie przekształciło się w główną mechanikę programu :D .
- **Visual Graph** -- Intuicyjny system tworzenia shader'ów wykorzystywanych przez materiały poprzez graf. W programie jest on wykorzystywany m. in. do wyświetlania topologii -- zarówno mapy ciepła jak i poziomic.
- **Input system** -- Oryginalny system obsługi sterowania pozostawiał wiele do życzenia. Wydany w 2019 r. jego następca rozwiązuje większość problemów i ułatwia pisanie sterowania w sposób elastyczny -- programista nie musi wiedzieć jakie klawisze odpowiadają za sterowanie, projektuje wyłącznie metody odpowiadające na wydarzenia generowane przez komponent zajmujący się input'em.
- **Mapa ciepła** -- Prosty przykład wizualizacji na jakie pozwala Unity -- wiedza, gdzie dron spędza najwięcej czasu, a jakie miejsca omija to cenna informacja dla projektantów AI.
- **Wizualizacja przepustnic** -- Użytkownik może obserwować każde polecenie algorytmu sterującego, czyli zmiany wartości przepustnic, patrząc na strzałki wychodzące z drona. Bardziej intuicyjne niż tekstowy wypis!
- **Visual Effect Graph** -- Oparty na obliczeniach na GPU system cząsteczek pozwalający na lekkie wydajnościowo a widowiskowe efekty wizualne. 

[ui toolkit]: <https://docs.unity3d.com/Manual/UIElements.html>
[shader graph]: <https://unity.com/shader-graph>
[input system]: <https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/manual/index.html>