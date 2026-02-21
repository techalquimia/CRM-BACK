package com.techalquimia.logistics.service;

import com.techalquimia.logistics.dto.EvidenceResponse;
import com.techalquimia.logistics.entity.Evidence;
import com.techalquimia.logistics.entity.EvidenceTypeCatalog;
import com.techalquimia.logistics.entity.TicketWeight;
import com.techalquimia.logistics.exception.EvidenceTypeNotFoundException;
import com.techalquimia.logistics.repository.EvidenceRepository;
import com.techalquimia.logistics.repository.EvidenceTypeCatalogRepository;
import com.techalquimia.logistics.service.TicketExtractionService.TicketData;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.time.Instant;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class EvidenceService {

    private final EvidenceRepository evidenceRepository;
    private final EvidenceTypeCatalogRepository evidenceTypeCatalogRepository;
    private final StorageService storageService;
    private final TicketExtractionService ticketExtractionService;

    @Transactional
    public EvidenceResponse createEvidence(UUID userId, MultipartFile image, Instant dateTime,
                                          Double latitude, Double longitude, String evidenceTypeCode) throws IOException {
        EvidenceTypeCatalog typeCatalog = evidenceTypeCatalogRepository.findByCode(evidenceTypeCode)
                .orElseThrow(() -> new EvidenceTypeNotFoundException(evidenceTypeCode));

        String imageUrl = storageService.uploadImage(image, "evidences", userId);

        Evidence evidence = Evidence.builder()
                .imageUrl(imageUrl)
                .dateTime(dateTime)
                .latitude(latitude)
                .longitude(longitude)
                .evidenceTypeCatalog(typeCatalog)
                .isSynced(false)
                .userId(userId)
                .build();

        if (Boolean.TRUE.equals(typeCatalog.getRequiresOcr())) {
            TicketData ticketData = ticketExtractionService.extractFromImage(image);
            TicketWeight ticketWeight = TicketWeight.builder()
                    .pesoTotal(ticketData.pesoTotal())
                    .tara(ticketData.tara())
                    .pesoNeto(ticketData.pesoNeto())
                    .evidence(evidence)
                    .build();
            evidence.setTicketWeight(ticketWeight);
        }

        evidence = evidenceRepository.save(evidence);
        return mapToResponse(evidence);
    }

    @Transactional(readOnly = true)
    public List<EvidenceResponse> getEvidencesByUser(UUID userId) {
        return evidenceRepository.findByUserIdOrderByCreatedAtDesc(userId)
                .stream()
                .map(this::mapToResponse)
                .collect(Collectors.toList());
    }

    @Transactional(readOnly = true)
    public List<EvidenceResponse> getEvidencesByUserAndType(UUID userId, String evidenceTypeCode) {
        return evidenceRepository.findByUserIdAndEvidenceTypeCatalog_CodeOrderByCreatedAtDesc(userId, evidenceTypeCode)
                .stream()
                .map(this::mapToResponse)
                .collect(Collectors.toList());
    }

    @Transactional(readOnly = true)
    public EvidenceResponse getById(UUID id, UUID userId) {
        Evidence evidence = evidenceRepository.findById(id).orElseThrow();
        if (!evidence.getUserId().equals(userId)) {
            throw new IllegalArgumentException("Evidencia no encontrada");
        }
        return mapToResponse(evidence);
    }

    @Transactional
    public EvidenceResponse markAsSynced(UUID id, UUID userId) {
        Evidence evidence = evidenceRepository.findById(id).orElseThrow();
        if (!evidence.getUserId().equals(userId)) {
            throw new IllegalArgumentException("Evidencia no encontrada");
        }
        evidence.setIsSynced(true);
        return mapToResponse(evidenceRepository.save(evidence));
    }

    private EvidenceResponse mapToResponse(Evidence e) {
        EvidenceTypeCatalog catalog = e.getEvidenceTypeCatalog();
        EvidenceResponse.EvidenceResponseBuilder builder = EvidenceResponse.builder()
                .id(e.getId())
                .imageUrl(e.getImageUrl())
                .dateTime(e.getDateTime())
                .latitude(e.getLatitude())
                .longitude(e.getLongitude())
                .evidenceType(catalog.getCode())
                .evidenceTypeName(catalog.getName())
                .isSynced(e.getIsSynced())
                .createdAt(e.getCreatedAt());

        if (e.getTicketWeight() != null) {
            TicketWeight tw = e.getTicketWeight();
            builder.pesoTotal(tw.getPesoTotal()).tara(tw.getTara()).pesoNeto(tw.getPesoNeto());
        }
        return builder.build();
    }
}
