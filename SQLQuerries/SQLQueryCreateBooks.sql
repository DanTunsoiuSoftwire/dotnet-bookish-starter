CREATE TABLE Books (
	id int PRIMARY KEY,
	title varchar(255) NOT NULL,
	isbn int NOT NULL,
	copies_owned int NOT NULL
);