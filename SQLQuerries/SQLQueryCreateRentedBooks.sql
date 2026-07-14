CREATE TABLE Rented_Books (
	id int PRIMARY KEY,
	book_id int,
	CONSTRAINT fk_rented_books
	FOREIGN KEY (book_id)
	REFERENCES Books(id),
	user__id int,
	CONSTRAINT fk_user
	FOREIGN KEY (user__id)
	REFERENCES Users(id),
	due_date date NOT NULL,
	borrowed_date date NOT NULL,
	returned_date date
);