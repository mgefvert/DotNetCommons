drop table if exists airports;
drop table if exists country_codes;
drop table if exists iana_ports;
drop table if exists nanp_area_codes;
drop table if exists zip_codes;

create table airports
(
    id           integer     not null primary key auto_increment,
    ident        varchar(10) not null,
    type         varchar(30) not null,
    icaocode     varchar(10),
    iatacode     varchar(10),
    name         varchar(255) not null,
    latitude     double      not null,
    longitude    double      not null,
    elevation    integer,
    continent    varchar(2)  not null,
    country      varchar(2)  not null,
    region       varchar(20),
    municipality varchar(255),
    key (ident),
    key (icaocode),
    key (iatacode),
    key (longitude),
    key (latitude),
    key (continent),
    key (country),
    key (region)
);

create table country_codes
(
    id        integer     not null primary key auto_increment,
    iso2      char(2)     not null,
    iso3      char(3)     not null,
    name      varchar(80) not null,
    capital   varchar(50),
    telcode   varchar(30),
    currency  varchar(30),
    continent char(2),
    region    varchar(50),
    subregion varchar(50),
    key (iso2),
    key (iso3),
    key (telcode)
);

create table iana_ports
(
    id           integer     not null primary key auto_increment,
    port         integer     not null,
    protocol     varchar(10) not null,
    name         varchar(50) not null,
    description  varchar(255),
    rfc          varchar(80),
    key (port),
    key (name)
);

create table nanp_area_codes
(
    id      integer     not null primary key auto_increment,
    code    varchar(3)  not null,
    country varchar(30) not null,
    state   varchar(50) null,
    key (code)
);

create table zip_codes
(
    id         integer     not null primary key auto_increment,
    code       varchar(5)  not null,
    city       varchar(80) not null,
    state      varchar(2)  not null,
    county     varchar(80) null,
    latitude   double      null,
    longitude  double      null,
    population int         null,
    key (code),
    key (state)
);
