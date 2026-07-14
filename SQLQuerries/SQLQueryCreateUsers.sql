CREATE TABLE Users (
	id int PRIMARY KEY,
	email varchar(255) NOT NULL,
	password_hash varchar(255) NOT NULL
);