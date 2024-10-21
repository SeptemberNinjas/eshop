    create table "catalog"(
        "id" int not null,
        "name" varchar(500) not null,
        "price" money not null,
        "type" smallint not null
    );

    create table "stock"(
        "id" int not null,
        "amount" int not null
    );

    insert into "catalog"("id", "name", "price", "type")
    values 
        (1, 'Лопата', 9.99, 1),
        (2, 'Трактор', 300, 1),
        (3, 'Раскопать яму', 5.49, 2),
        (4, 'Вспахать поле', 1000, 2);

    insert into "stock"("id", "amount")
    values
        (1, 3),
        (2, 4);