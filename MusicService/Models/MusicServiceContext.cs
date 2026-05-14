using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MusicService.Models;

public partial class MusicServiceContext : DbContext
{
    public MusicServiceContext()
    {
    }

    public MusicServiceContext(DbContextOptions<MusicServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Album> Albums { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<PlaylistTrack> PlaylistTracks { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("music");
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("account_pkey");

            entity.ToTable("account");

            entity.HasIndex(e => e.Email, "account_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(50)
                .HasColumnName("patronymic");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
        });

        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("action_pkey");

            entity.ToTable("action");

            entity.HasIndex(e => new { e.AccountId, e.Weight }, "action_weight_index").HasFilter("(weight > (0)::numeric)");

            entity.HasIndex(e => new { e.AccountId, e.TrackId }, "unique_user_track_action").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.IsLike).HasColumnName("is_like");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Weight)
                .HasPrecision(3, 2)
                .HasColumnName("weight");

            entity.HasOne(d => d.Account).WithMany(p => p.Actions)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("action_account_id_fkey");

            entity.HasOne(d => d.Track).WithMany(p => p.Actions)
                .HasForeignKey(d => d.TrackId)
                .HasConstraintName("action_track_id_fkey");
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("album_pkey");

            entity.ToTable("album");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.CoverPath)
                .HasMaxLength(255)
                .HasColumnName("cover_path");
            entity.Property(e => e.ReleaseDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("release_date");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");

            entity.HasOne(d => d.Artist).WithMany(p => p.Albums)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("album_artist_id_fkey");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("artist_pkey");

            entity.ToTable("artist");

            entity.HasIndex(e => e.Name, "artist_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genre_pkey");

            entity.ToTable("genre");

            entity.HasIndex(e => e.Name, "genre_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("playlist_pkey");

            entity.ToTable("playlist");

            entity.HasIndex(e => e.AccountId, "playlist_user_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CoverPath)
                .HasMaxLength(255)
                .HasColumnName("cover_path");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");

            entity.HasOne(d => d.Account).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("playlist_account_id_fkey");
        });

        modelBuilder.Entity<PlaylistTrack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("playlist_tracks_pkey");

            entity.ToTable("playlist_tracks");

            entity.HasIndex(e => new { e.PlaylistId, e.TrackId }, "unique_playlist_track").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("added_at");
            entity.Property(e => e.PlaylistId).HasColumnName("playlist_id");
            entity.Property(e => e.TrackId).HasColumnName("track_id");

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistTracks)
                .HasForeignKey(d => d.PlaylistId)
                .HasConstraintName("playlist_tracks_playlist_id_fkey");

            entity.HasOne(d => d.Track).WithMany(p => p.PlaylistTracks)
                .HasForeignKey(d => d.TrackId)
                .HasConstraintName("playlist_tracks_track_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("track_pkey");

            entity.ToTable("track");

            entity.HasIndex(e => e.GenreId, "track_genre_index");

            entity.HasIndex(e => e.Title, "track_title_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlbumId).HasColumnName("album_id");
            entity.Property(e => e.CoverPath)
                .HasMaxLength(255)
                .HasColumnName("cover_path");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .HasColumnName("file_path");
            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");

            entity.HasOne(d => d.Album).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.AlbumId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("track_album_id_fkey");

            entity.HasOne(d => d.Genre).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("track_genre_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<int> CreateAlbum(string title, string artistName, string? coverPath = null, DateTime? releaseDate = null)
    {
        // 1. Берем дату. Если она не введена - берем текущую.
        // 2. Вызываем .Date, чтобы сбросить время в 00:00:00.
        DateTime dateValue = (releaseDate ?? DateTime.Now).Date;

        // 3. Явно помечаем дату как UTC. 
        // Это гарантирует, что Postgres воспримет её корректно и она не "улетит" в будущее.
        DateTime utcDate = DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);

        return await Database
            .SqlQueryRaw<int>("SELECT create_album({0}, {1}, {2}, {3}) AS \"Value\"",
                title,
                artistName,
                coverPath ?? (object)DBNull.Value,
                utcDate)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UploadTrack(string title, string artist, string album, string path, TimeSpan dur, string genre, string? cover = null)
    {
        return await Database.SqlQueryRaw<int>(
            "SELECT upload_track({0}, {1}, {2}, {3}, {4}, {5}, {6}) AS \"Value\"", title, artist, album, path, dur, genre, cover ?? (object)DBNull.Value)
            .FirstOrDefaultAsync();
    }

    public async Task AddTrackToPlaylist(int playlistId, int trackId)
    {
        try
        {
            await Database.ExecuteSqlRawAsync("SELECT music.add_track_to_playlist({0}, {1})", playlistId, trackId);
        }
        catch (Npgsql.PostgresException ex)
        {
            throw new Exception(ex.MessageText);
        }
    }

    public async Task RegisterUserAction(int accountId, int trackId, bool isLike)
    {
        await Database.ExecuteSqlRawAsync("SELECT music.register_user_action({0}, {1}, {2})", accountId, trackId, isLike);
    }

    public async Task<List<Track>> GetRecommendations(int accountId)
    {
        var trackIds = await Database.SqlQueryRaw<int>("SELECT track_id FROM music.get_recommendations({0})", accountId).ToListAsync();
        var tracks = await Tracks.Where(t => trackIds.Contains(t.Id)).Include(t => t.Album).ThenInclude(a => a.Artist).Include(t => t.Genre).ToListAsync();
        return tracks.OrderBy(t => trackIds.IndexOf(t.Id)).ToList();
    }
    public async Task DeleteUserAction(int accountId, int trackId)
    {
        await Database.ExecuteSqlRawAsync("DELETE FROM action WHERE account_id = {0} AND track_id = {1}", accountId, trackId);
    }
}
