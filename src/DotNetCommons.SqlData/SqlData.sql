drop table if exists geo_airports;
drop table if exists geo_area_codes;
drop table if exists geo_countries;
drop table if exists geo_zip_codes;

create table geo_airports
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

create table geo_area_codes
(
    id      integer     not null primary key auto_increment,
    code    varchar(3)  not null,
    country varchar(30) not null,
    state   varchar(50) null,
    key (code)
);

create table geo_countries
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

create table geo_zip_codes
(
    id         integer     not null primary key auto_increment,
    code       varchar(5)  not null,
    city       varchar(80) not null,
    state      varchar(2)  not null,
    county     varchar(80) null,
    latitude   double      null,
    longitude  double      null,
    key (code),
    key (state)
);

create table ip_city (
    ip      binary(16) not null,
    country int        null,
    state   int        null,
    city    int        null,
    primary key (ip)
)
    row_format=compressed;

create table ip_country (
    ip      binary(16) not null,
    country int        null,
    primary key (ip)
)
    row_format=compressed;

create table ip_lookup (
    id   int          not null auto_increment,
    name varchar(255) not null,
    primary key (id)
);
