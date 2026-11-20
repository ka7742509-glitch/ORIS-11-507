-- PostgreSQL
CREATE TABLE CardSets (
                          Id SERIAL PRIMARY KEY,
                          Code VARCHAR(10) NOT NULL UNIQUE,
                          Name VARCHAR(200) NOT NULL,
                          ReleaseDate DATE NOT NULL,
                          CardCount INT NOT NULL
);

CREATE TABLE Artists (
                         Id SERIAL PRIMARY KEY,
                         Name VARCHAR(200) NOT NULL,
                         Country VARCHAR(100),
                         Bio TEXT
);

CREATE TABLE Rarities (
                          Id SERIAL PRIMARY KEY,
                          Code VARCHAR(20) NOT NULL UNIQUE,
                          Name VARCHAR(50) NOT NULL,
                          SortOrder INT NOT NULL
);

CREATE TABLE Cards (
                       Id SERIAL PRIMARY KEY,
                       Number INT NOT NULL,
                       Name VARCHAR(200) NOT NULL,
                       ManaCost VARCHAR(100),
                       TypeLine VARCHAR(200) NOT NULL,
                       RarityId INT NOT NULL REFERENCES Rarities(Id),
                       SetId INT NOT NULL REFERENCES CardSets(Id),
                       ArtistId INT NOT NULL REFERENCES Artists(Id),
                       RulesText TEXT,
                       FlavorText TEXT,
                       Power VARCHAR(10),
                       Toughness VARCHAR(10),
                       Language VARCHAR(20) NOT NULL DEFAULT 'English',
                       LegalFormats TEXT,
                       ImageUrl VARCHAR(500),
                       UNIQUE (SetId, Number)
);

CREATE INDEX IX_Cards_Name ON Cards(Name);
CREATE INDEX IX_Cards_TypeLine ON Cards(TypeLine);