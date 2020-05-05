drop table is_on_requests;
create table is_on_requests
(
    id          INTEGER
        constraint is_on_requests_pk
            primary key autoincrement,
    created     DATETIME not null,
    device_id   INTEGER,
    error_count INTEGER,
    pump_state  INTEGER,
    raw_temp    INTEGER,
    response    BOOLEAN  not null
);

drop table waterpump;
create table waterpump
(
    id            INTEGER
        constraint waterpump_pk
            primary key autoincrement,
    is_forever_on boolean  not null,
    is_on_until   DATETIME not null
);

insert into waterpump (is_forever_on, is_on_until)
VALUES (false, DATETIME());