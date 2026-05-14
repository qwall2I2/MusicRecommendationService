-- СОЗДАНИЕ СХЕМЫ
CREATE SCHEMA IF NOT EXISTS music;
SET search_path = music;

-- ДОМЕНЫ
CREATE DOMAIN correct_email AS VARCHAR(50)
CHECK (VALUE ~* '^[A-Za-z0-9._-]+@[A-Za-z0-9.-]+\.[A-Za-z]+$');

-- ТАБЛИЦЫ
CREATE TABLE role (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL CHECK (name IN ('пользователь', 'администратор'))
);

CREATE TABLE account (
    id SERIAL PRIMARY KEY,
    email correct_email NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    patronymic VARCHAR(50),
    role_id INTEGER NOT NULL REFERENCES role(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE artist (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE genre (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE album (
    id SERIAL PRIMARY KEY,
    title VARCHAR(50) NOT NULL,
    artist_id INTEGER NOT NULL REFERENCES artist(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    release_date TIMESTAMP NOT NULL DEFAULT NOW() CHECK (release_date <= NOW()),
    cover_path VARCHAR(255)
);

CREATE TABLE track (
    id SERIAL PRIMARY KEY,
    title VARCHAR(50) NOT NULL,
    duration INTERVAL NOT NULL CHECK (duration > '0 seconds'),
    file_path VARCHAR(255) NOT NULL,
    album_id INTEGER NOT NULL REFERENCES album(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    genre_id INTEGER NOT NULL REFERENCES genre(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    cover_path VARCHAR(255)
);

CREATE TABLE playlist (
    id SERIAL PRIMARY KEY,
    title VARCHAR(50) NOT NULL,
    cover_path VARCHAR(255),
    account_id INTEGER NOT NULL REFERENCES account(id) ON DELETE CASCADE ON UPDATE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW() CHECK (created_at <= NOW())
);

CREATE TABLE playlist_tracks (
    id SERIAL PRIMARY KEY,
    playlist_id INTEGER NOT NULL REFERENCES playlist(id) ON DELETE CASCADE ON UPDATE CASCADE,
    track_id INTEGER NOT NULL REFERENCES track(id) ON DELETE CASCADE ON UPDATE CASCADE,
    added_at TIMESTAMP NOT NULL DEFAULT NOW() CHECK (added_at <= NOW()),
    CONSTRAINT unique_playlist_track UNIQUE (playlist_id, track_id)
);

CREATE TABLE action (
    id SERIAL PRIMARY KEY,
    account_id INTEGER NOT NULL REFERENCES account(id) ON DELETE CASCADE ON UPDATE CASCADE,
    track_id INTEGER NOT NULL REFERENCES track(id) ON DELETE CASCADE ON UPDATE CASCADE,
    is_like BOOLEAN NOT NULL,
    weight DECIMAL(3, 2) NOT NULL CHECK (weight >= 0),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW() CHECK (updated_at <= NOW()),
    CONSTRAINT unique_user_track_action UNIQUE (account_id, track_id)
);

-- ИНДЕКСЫ
CREATE INDEX track_title_index ON track(title);
CREATE INDEX track_genre_index ON track(genre_id);
CREATE INDEX action_weight_index ON action(account_id, weight) WHERE weight > 0;
CREATE INDEX playlist_user_index ON playlist(account_id);

-- ФУНКЦИИ

-- добавление в плейлист
CREATE OR REPLACE FUNCTION add_track_to_playlist(p_playlist_id int, p_track_id int)
RETURNS void AS $$
DECLARE
    track_count int;
BEGIN
    IF EXISTS (SELECT 1 FROM playlist_tracks WHERE playlist_id = p_playlist_id AND track_id = p_track_id) THEN
        RAISE EXCEPTION 'этот трек уже есть в данном плейлисте';
    END IF;
    SELECT count(*) INTO track_count FROM playlist_tracks WHERE playlist_id = p_playlist_id;
    IF track_count >= 500 THEN
        RAISE EXCEPTION 'превышен лимит треков в плейлисте (максимум 500).';
    END IF;
    INSERT INTO playlist_tracks (playlist_id, track_id) VALUES (p_playlist_id, p_track_id);
END;
$$ LANGUAGE plpgsql;

-- регистрация оценки
CREATE OR REPLACE FUNCTION register_user_action(p_account_id int, p_track_id int, p_is_like boolean)
RETURNS void AS $$
DECLARE
    v_weight decimal(3, 2);
BEGIN
    v_weight := CASE WHEN p_is_like THEN 1.20 ELSE 0.00 END;
    IF EXISTS (SELECT 1 FROM action WHERE account_id = p_account_id AND track_id = p_track_id) THEN
        UPDATE action SET is_like = p_is_like, weight = v_weight, updated_at = now()
        WHERE account_id = p_account_id AND track_id = p_track_id;
    ELSE
        INSERT INTO action (account_id, track_id, is_like, weight, updated_at)
        VALUES (p_account_id, p_track_id, p_is_like, v_weight, now());
    END IF;
END;
$$ LANGUAGE plpgsql;

-- рекомендации
CREATE OR REPLACE FUNCTION get_recommendations(p_account_id int)
RETURNS TABLE(track_id int, title varchar, artist_name varchar, cover_path varchar, file_path varchar)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    WITH user_genres AS (
        SELECT t.genre_id, SUM(a.weight) as score FROM action a
        JOIN track t ON a.track_id = t.id
        WHERE a.account_id = p_account_id AND a.weight > 0
        GROUP BY t.genre_id ORDER BY score DESC LIMIT 3
    ),
    new_tracks AS (
        SELECT tr.id, tr.title, art.name, tr.cover_path, tr.file_path, 1 as priority
        FROM track tr JOIN album alb ON tr.album_id = alb.id JOIN artist art ON alb.artist_id = art.id
        WHERE tr.genre_id IN (SELECT genre_id FROM user_genres)
        AND tr.id NOT IN (SELECT a.track_id FROM action a WHERE a.account_id = p_account_id)
    ),
    old_likes AS (
        SELECT tr.id, tr.title, art.name, tr.cover_path, tr.file_path, 2 as priority
        FROM action a JOIN track tr ON a.track_id = tr.id JOIN album alb ON tr.album_id = alb.id JOIN artist art ON alb.artist_id = art.id
        WHERE a.account_id = p_account_id AND a.is_like = true
    )
    SELECT r.id, r.title, r.name, r.cover_path, r.file_path FROM (SELECT * FROM new_tracks UNION ALL SELECT * FROM old_likes) r
    ORDER BY r.priority ASC, random() LIMIT 10;
END;
$$;

-- создание альбома
CREATE OR REPLACE FUNCTION create_album(p_title varchar, p_artist_name varchar, p_cover varchar DEFAULT NULL, p_date timestamp DEFAULT NOW())
RETURNS int AS $$
DECLARE
    v_art_id int; v_alb_id int;
BEGIN
    SELECT id INTO v_art_id FROM artist WHERE name = p_artist_name;
    IF v_art_id IS NULL THEN INSERT INTO artist (name) VALUES (p_artist_name) RETURNING id INTO v_art_id; END IF;
    SELECT id INTO v_alb_id FROM album WHERE title = p_title AND artist_id = v_art_id;
    IF v_alb_id IS NULL THEN
        INSERT INTO album (title, artist_id, cover_path, release_date)
        VALUES (p_title, v_art_id, COALESCE(p_cover, '/uploads/covers/default_cover.jpg'), p_date) RETURNING id INTO v_alb_id;
    END IF;
    RETURN v_alb_id;
END;
$$ LANGUAGE plpgsql;

-- загрузка трека
CREATE OR REPLACE FUNCTION upload_track(p_title varchar, p_artist varchar, p_album varchar, p_file varchar, p_dur interval, p_genre varchar, p_cover varchar DEFAULT NULL)
RETURNS int AS $$
DECLARE
    v_alb_id int; v_gen_id int; v_tr_id int; v_alb_cover varchar;
BEGIN
    SELECT id, cover_path INTO v_alb_id, v_alb_cover FROM album WHERE title = p_album;
    SELECT id INTO v_gen_id FROM genre WHERE name = p_genre;
    IF v_gen_id IS NULL THEN INSERT INTO genre (name) VALUES (p_genre) RETURNING id INTO v_gen_id; END IF;
    INSERT INTO track (title, duration, file_path, album_id, genre_id, cover_path)
    VALUES (p_title, p_dur, p_file, v_alb_id, v_gen_id, COALESCE(p_cover, v_alb_cover, '/uploads/covers/default_cover.jpg')) RETURNING id INTO v_tr_id;
    RETURN v_tr_id;
END;
$$ LANGUAGE plpgsql;

-- ТРИГГЕРЫ
CREATE OR REPLACE FUNCTION check_playlists_limit_func() RETURNS TRIGGER AS $$
BEGIN
    IF (SELECT count(*) FROM playlist WHERE account_id = NEW.account_id) >= 50 THEN
        RAISE EXCEPTION 'превышен лимит плейлистов (максимум 50)';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_playlist_limit BEFORE INSERT ON playlist FOR EACH ROW EXECUTE FUNCTION check_playlists_limit_func();

-- ДАННЫЕ
INSERT INTO role (name) VALUES ('администратор'), ('пользователь');
INSERT INTO genre (name) VALUES ('Рок'), ('Поп'), ('Джаз'), ('Хип-хоп'), ('Классика');
INSERT INTO account (email, password, first_name, last_name, role_id) 
VALUES ('admin@admin.admin', 'admin', 'admin', 'admin', 1);