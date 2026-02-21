package com.techalquimia.logistics.repository;

import com.techalquimia.logistics.entity.Evidence;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.UUID;

public interface EvidenceRepository extends JpaRepository<Evidence, UUID> {

    List<Evidence> findByUserIdOrderByCreatedAtDesc(UUID userId);

    List<Evidence> findByUserIdAndEvidenceTypeCatalog_CodeOrderByCreatedAtDesc(UUID userId, String evidenceTypeCode);
}
