﻿using Kibol_Alert.Database;
using Kibol_Alert.Models;
using Kibol_Alert.Requests;
using Kibol_Alert.Responses;
using Kibol_Alert.Services.Interfaces;
using Kibol_Alert.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kibol_Alert.Services
{
    public class ClubService : BaseService, IClubService
    {
        public ClubService(Kibol_AlertContext context, ILoggerService logger) : base(context, logger)
        { }

        public async Task<Response> AddClub(ClubRequest request)
        {
            var club = new Club()
            {
                Name = request.Name,
                League = request.League,
                LogoUri = request.LogoUri
            };

            await Context.Clubs.AddAsync(club);
            await Context.SaveChangesAsync();

            AddLog($"Dodano klub {request.Name}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> DeleteClub(int id)
        {
            var club = await Context.Clubs.FirstOrDefaultAsync(i => i.Id == id);
            if (club == null)
            {
                return new ErrorResponse("Klubu nie znaleziono!");
            }
            club.IsDeleted = true;
            await Context.SaveChangesAsync();

            AddLog($"Usunięto klub {club.Name} o id {club.Id}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> EditClub(int id, ClubRequest request)
        {
            var club = await Context.Clubs.FirstOrDefaultAsync(i => i.Id == id);

            if (club == null)
            {
                return new ErrorResponse("Klubu nie znaleziono!");
            }
            if (request.Name != null)
                club.Name = request.Name;
            if (request.League != null)
                club.League = request.League;
            if (request.LogoUri != null)
                club.LogoUri = request.LogoUri;
            if (request.Longitude != null)
                club.Longitude = request.Longitude;
            if (request.Latitude != null)
                club.Latitude = request.Latitude;

            Context.Clubs.Update(club);

            await Context.SaveChangesAsync();
            AddLog($"Edytowano klub {club.Id}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> AddChant(ClubChantRequest request)
        {
            var chant = new Chant(){
                ClubId = request.ClubId,
                Lyrics = request.Lyrics
            };
            await Context.Chants.AddAsync(chant);
            await Context.SaveChangesAsync();
            AddLog($"Dodano przyśpiewke {request}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> EditChant(int id, ClubChantRequest request)
        {
            var chant = await Context.Chants.FirstOrDefaultAsync(i => i.Id == id);
            if (chant == null)
            {
                return new ErrorResponse("Przyśpiewki nie znaleziono!");
            }
            chant.Lyrics = request.Lyrics;
            Context.Chants.Update(chant);
            await Context.SaveChangesAsync();
            AddLog($"Edytowano przyśpiewke {chant.Id}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> DeleteChant(int id)
        {
            var chant = await Context.Chants.FirstOrDefaultAsync(i => i.Id == id);
            if (chant == null)
            {
                return new ErrorResponse("Przyśpiewki nie znaleziono!");
            }
            Context.Chants.Remove(chant);
            AddLog($"Usunięto przyśpiewke {chant.Id}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> AddRelation(ClubRelationRequest request)
        {
            var clubRelation = new ClubRelation()
            {
                FirstClubId = request.FirstClubId,
                SecondClubId = request.SecondClubId,
                Relation = request.Relation
            };

            var clubRelation2 = new ClubRelation()
            {
                FirstClubId = request.SecondClubId,
                SecondClubId = request.FirstClubId,
                Relation = request.Relation
            };

            await Context.ClubRelations.AddAsync(clubRelation);
            await Context.ClubRelations.AddAsync(clubRelation2);
            await Context.SaveChangesAsync();

            AddLog($"Dodano relacje pomiędzy {request.FirstClubId} i {request.SecondClubId}");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> DeleteRelation(int id)
        {
            var clubRelation = await Context.ClubRelations.FirstOrDefaultAsync(i => i.FirstClub.Id == id);
            if (clubRelation == null)
            {
                return new ErrorResponse("Relacji nie znaleziono!");
            }
            Context.ClubRelations.Remove(clubRelation);
            await Context.SaveChangesAsync();
            AddLog($"Usunięto relacje pomiędzy {clubRelation.FirstClubId} i {clubRelation.SecondClubId} ");
            return new SuccessResponse<bool>(true);
        }

        public async Task<Response> GetClub(int id)
        {
            var club = await Context.Clubs
                .Include(i => i.RelationsWith)
                .Include(i => i.InRelationsWith).ThenInclude(relation => relation.FirstClub)
                .Include(i => i.Fans)
                .Include(i => i.Chants)
                .FirstOrDefaultAsync(i => !i.IsDeleted && i.Id == id);

            var clubDto = new ClubVM()
            {
                Id = club.Id,
                Name = club.Name,
                League = club.League,
                LogoUri = club.LogoUri,
                City = club.City,
                Longitude = club.Longitude,
                Latitude = club.Latitude,

                Chants = club.Chants.Select(row => new ChantVM()
                {
                    Id = row.Id,
                    Lyrics = row.Lyrics
                }).ToList(),

                ClubRelations = club.InRelationsWith.Select(row => new ClubRelationVM()
                { 
                    ClubId = row.FirstClub.Id,
                    ClubName = row.FirstClub.Name,
                    Relation = row.Relation
                }) .ToList(),

                Fans = club.Fans.Select(row => new MemberVM()
                {
                    MemberId = row.Id,
                    Name = row.UserName,
                }).ToList(),
            };

            return new SuccessResponse<ClubVM>(clubDto);
        }

        public async Task<Response> GetClubs(int skip, int take)
        {
            var clubs = await Context.Clubs
                .Where(i => !i.IsDeleted)
                .Skip(skip)
                .Take(take)
                .Include(i => i.RelationsWith)
                .Include(i => i.InRelationsWith)
                .Include(i => i.Fans)
                .Include(i => i.Chants)
                .Select(row => new ClubVM()
                {
                    Id = row.Id,
                    Name = row.Name,
                    League = row.League,
                    LogoUri = row.LogoUri,
                    City = row.City,
                    Longitude = row.Longitude,
                    Latitude = row.Latitude,
                    Chants = row.Chants.Select(row=> new ChantVM()
                    {
                        Id = row.Id,
                        Lyrics = row.Lyrics
                    }).ToList(),
                    ClubRelations = row.InRelationsWith.Select(row => new ClubRelationVM()
                    {
                        ClubId = row.FirstClub.Id,
                        ClubName = row.FirstClub.Name,
                        Relation = row.Relation
                    }).ToList(),

                    Fans = row.Fans.Select(row => new MemberVM()
                    {
                        MemberId = row.Id,
                        Name = row.UserName,
                    }).ToList(),
                }).ToListAsync();

            return new SuccessResponse<List<ClubVM>>(clubs);
        }
    }
}