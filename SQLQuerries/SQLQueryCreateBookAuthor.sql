CREATE TABLE Book_Author (
	id int PRIMARY KEY,
	book_id int,
	CONSTRAINT fk_Book
	FOREIGN KEY (book_id)
	REFERENCES Books(id),
	author_id int,
	CONSTRAINT fk_author
	FOREIGN KEY (author_id)
	REFERENCES Authors(id)
);