using Microsoft.AspNetCore.Mvc;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Domain.Entities;

namespace QnaIntelligence.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FollowedTagsController(IFollowedTagRepository followedTagRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FollowedTagDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tags = await followedTagRepository.GetAllAsync(cancellationToken);
        return tags.Select(ToDto).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FollowedTagDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var followedTag = await followedTagRepository.GetByIdAsync(id, cancellationToken);
        return followedTag is null ? NotFound() : Ok(ToDto(followedTag));
    }

    [HttpPost]
    public async Task<ActionResult<FollowedTagDto>> Create(
        CreateFollowedTagDto request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Tag name is required.");
        }

        if (await followedTagRepository.NameExistsAsync(name, null, cancellationToken))
        {
            return Conflict("A followed tag with this name already exists.");
        }

        var followedTag = new FollowedTag
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };

        await followedTagRepository.AddAsync(followedTag, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = followedTag.Id },
            ToDto(followedTag));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FollowedTagDto>> Update(
        Guid id,
        UpdateFollowedTagDto request,
        CancellationToken cancellationToken)
    {
        var followedTag = await followedTagRepository.GetByIdAsync(id, cancellationToken);
        if (followedTag is null)
        {
            return NotFound();
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Tag name is required.");
        }

        if (await followedTagRepository.NameExistsAsync(name, id, cancellationToken))
        {
            return Conflict("A followed tag with this name already exists.");
        }

        followedTag.Name = name;
        followedTag.IsActive = request.IsActive;
        await followedTagRepository.UpdateAsync(followedTag, cancellationToken);

        return Ok(ToDto(followedTag));
    }

    private static FollowedTagDto ToDto(FollowedTag followedTag)
    {
        return new FollowedTagDto
        {
            Id = followedTag.Id,
            Name = followedTag.Name,
            IsActive = followedTag.IsActive,
            CreatedUtc = followedTag.CreatedUtc
        };
    }
}
